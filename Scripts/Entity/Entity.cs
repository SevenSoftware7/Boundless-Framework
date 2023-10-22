using System.Diagnostics.CodeAnalysis;
using Godot;
using SevenGame.Utility;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public sealed partial class Entity : CharacterBody3D {
    
    [ExportGroup("Dependencies")]
    
    [Export] public EntityBehaviourManager? BehaviourManager { 
        get => _behaviourManager ??= this.GetNodeByTypeName<EntityBehaviourManager>();
        private set => _behaviourManager = value;
    }
    private EntityBehaviourManager? _behaviourManager;

    [Export] public Health? Health { 
        get => _health ??= this.GetNodeByTypeName<Health>();
        private set => _health = value;
    }
    private Health? _health;

    [Export] public WeaponInventory? WeaponInventory { 
        get {
            if ( _weaponInventory is null ) {
                _weaponInventory = this.GetNodeByTypeName<WeaponInventory>();

                if (_weaponInventory is null) return null;

                if ( Armature is not null ) {
                    _weaponInventory.SkeletonPath = _weaponInventory.GetPathTo(Armature);
                }
                _weaponInventory.ReloadModel(Character?.IsLoaded ?? false);
            }

            return _weaponInventory;
        }
        private set => _weaponInventory = value;
    }
    private WeaponInventory? _weaponInventory;



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

    [Export] public CharacterData? CharacterData {
        get => Character?.Data;
        private set => this.CallDeferredIfTools( Callable.From(() => SetCharacter(value)) );
    }

    [Export] public CharacterCostume? CharacterCostume {
        get => Character?.CharacterCostume;
        private set => this.CallDeferredIfTools( Callable.From(() => SetCostume(value)) );
    }

    
    public Skeleton3D? Armature => Character?.Armature;


    [Signal] public delegate void CharacterChangedEventHandler(CharacterData? newCharacter, CharacterData? oldCharacter);


    public Entity() : base() {
        CollisionLayer = 1 << 1;

        _behaviourManager ??= null !;
        Character ??= null !;
    }



    public void SetCharacter(CharacterData? data, CharacterCostume? costume = null) {
        if ( this.IsInvalidTreeCallback() ) return;
        if ( CharacterData == data ) return;

        Character?.QueueFree();
        Character = null !;

        CharacterData? oldData = CharacterData;
        if ( data is not null ) {
            Character = data.Instantiate(this);
            Character?.LoadModel();
        }
        EmitSignal(SignalName.CharacterChanged, data!, oldData!);

        if ( data is not null ) {
            SetCostume(costume ?? data.BaseCostume);
        }
    }

    public void SetCostume(CharacterCostume? costume) {
        Character?.SetCostume(costume);
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

    private void OnCharacterLoadChanged(bool isLoaded) {
        if ( _weaponInventory is null ) return;

        _weaponInventory.SkeletonPath = Armature is not null ? _weaponInventory.GetPathTo(Armature) : new();
        _weaponInventory.ReloadModel();
    }

    private void OnCharacterChanged(CharacterData? newCharacter, CharacterData? oldCharacter) {
        Character.ModelLoaded += OnCharacterLoadChanged;
        OnCharacterLoadChanged(Character.IsLoaded);
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

        BehaviourManager?.SetBehaviour<TestBehaviour>(
            () => new(this, BehaviourManager) {
                Name = nameof(TestBehaviour)
            }
        );
	}

}
