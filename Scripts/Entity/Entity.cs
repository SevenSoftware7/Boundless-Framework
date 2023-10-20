using Godot;
using SevenGame.Utility;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public sealed partial class Entity : CharacterBody3D {
    
    [ExportGroup("Dependencies")]
    
    [Export] public EntityBehaviourManager BehaviourManager { 
        get {
            _behaviourManager ??= this.GetNodeByTypeName<EntityBehaviourManager>();
            if ( _behaviourManager is null ) {
                _behaviourManager = new(this);
                this.AddChildSetOwner(_behaviourManager);
            }

            return _behaviourManager;
        }
        private set => _behaviourManager = value;
    }
    private EntityBehaviourManager _behaviourManager;

    [Export] public Health Health { 
        get {
            _health ??= this.GetNodeByTypeName<Health>();
            if ( _health is null ) {
                _health = new(this);
                this.AddChildSetOwner(_health);
            }

            return _health;
        }
        private set => _health = value;
    }
    private Health _health;

    [Export] public WeaponInventory WeaponInventory { 
        get {
            _weaponInventory ??= this.GetNodeByTypeName<WeaponInventory>();
            if ( _weaponInventory is not null ) {
                if ( Skeleton is not null ) {
                    _weaponInventory.SkeletonPath = _weaponInventory.GetPathTo(Skeleton);
                }
                // _weaponInventory.ReloadModel(Character?.IsLoaded ?? false);
            }

            return _weaponInventory;
        }
        private set => _weaponInventory = value;
    }
    private WeaponInventory _weaponInventory;



    [ExportGroup("Movement")]

    [Export] public Vector3 Inertia = Vector3.Zero; 
    [Export] public Vector3 Movement = Vector3.Zero; 


    /// <summary>
    /// The forward direction in absolute space of the Entity.
    /// </summary>
    /// <remarks>
    /// Editing this value also changes <see cref="RelativeForward"/> to match.
    /// </remarks>
    [Export] public Vector3 AbsoluteForward { 
        get => _absoluteForward; 
        set { 
            _absoluteForward = value.Normalized(); 
            _relativeForward = Transform.Basis.Inverse() * _absoluteForward; 
        } 
    }
    private Vector3 _absoluteForward;


    /// <summary>
    /// The forward direction in relative space of the Entity.
    /// </summary>
    /// <remarks>
    /// Editing this value also changes <see cref="AbsoluteForward"/> to match.
    /// </remarks>
    [Export] public Vector3 RelativeForward { 
        get => _relativeForward; 
        set { 
            _relativeForward = value.Normalized();
            _absoluteForward = Transform.Basis * _relativeForward;
        }
    }
    private Vector3 _relativeForward;

    [ExportGroup("")]


    [Export] public Character Character { get; private set; }

    [Export] public CharacterData CharacterData {
        get => Character?.Data;
        private set => this.CallDeferredIfTools( Callable.From(() => SetCharacter(value)) );
    }

    [Export] public CharacterCostume CharacterCostume {
        get => Character?.CharacterCostume;
        private set => this.CallDeferredIfTools( Callable.From(() => SetCostume(value)) );
    }

    
    public Skeleton3D Skeleton => Character?.Skeleton;


    [Signal] public delegate void CharacterChangedEventHandler(CharacterData newCharacter, CharacterData oldCharacter);
    [Signal] public delegate void CostumeChangedEventHandler(CharacterCostume newCostume, CharacterCostume oldCostume);


    public Entity() : base() {
        CollisionLayer = 1 << 1;
    }



    public void SetCharacter(CharacterData data, CharacterCostume costume = null) {
        if ( this.IsInvalidTreeCallback() ) return;
        if ( CharacterData == data ) return;

        // Character?.UnloadModel();
        Character?.QueueFree();
        Character = null;

        CharacterData oldData = CharacterData;
        if ( data is not null ) {
            Character = data?.Instantiate(this);
            Character?.LoadModel();
        }
        EmitSignal(SignalName.CharacterChanged, data, oldData);

        if ( data is not null ) {
            SetCostume(costume ?? data?.BaseCostume);
        }
    }

    public void SetCostume(CharacterCostume costume) {
        Character?.SetCostume(costume);
        EmitSignal(SignalName.CostumeChanged, costume);
    }


    public void HandleInput(Player.InputInfo inputInfo) {
        BehaviourManager?.CurrentBehaviour?.HandleInput(inputInfo);
    }


    private bool ProcessInertia(out Vector3 verticalInertia, out Vector3 horizontalInertia, double delta) {

        float floatDelta = (float)delta;

        verticalInertia = Inertia.Project( UpDirection );
        horizontalInertia = Inertia - verticalInertia;

        Vector3 gravityForce = -UpDirection * 15f; // TODO: replace with getter

        verticalInertia = verticalInertia.MoveToward( gravityForce, 35f * floatDelta );
        horizontalInertia = horizontalInertia.MoveToward( Vector3.Zero, 0.25f * floatDelta );

        if ( MotionMode == MotionModeEnum.Grounded ) {
            // if ( IsOnFloor() ) verticalInertia = verticalInertia.FlattenInDirection( -UpDirection );
            // if ( IsOnCeiling() ) verticalInertia = verticalInertia.FlattenOnPlane( -UpDirection );
        }

        return true;
    }

    private void OnCharacterChanged(CharacterData newCharacter, CharacterData oldCharacter) {        
        if ( _weaponInventory is null ) return;

        _weaponInventory.SkeletonPath = Skeleton is null ? null : _weaponInventory.GetPathTo(Skeleton);
        _weaponInventory.ReloadModel(oldCharacter is null);
    }


    public override void _EnterTree() {
        base._EnterTree();
        CharacterChanged += OnCharacterChanged;
    }

    public override void _ExitTree() {
        base._ExitTree();
        CharacterChanged -= OnCharacterChanged;
    }

    public override void _Process(double delta) {
        base._Process(delta);

        if ( Engine.IsEditorHint() ) return;
        
        // if ( GravityMultiplier != 0f & Weight != 0f ) {
            ProcessInertia(out Vector3 verticalInertia, out Vector3 horizontalInertia, delta);
            Inertia = horizontalInertia + verticalInertia;
        // }

        Velocity = Movement + Inertia;
        MoveAndSlide();
    }

    public override void _Ready() {
        base._Ready();

        if ( Engine.IsEditorHint() ) return;

        BehaviourManager.SetBehaviour<TestBehaviour>();
	}

}
