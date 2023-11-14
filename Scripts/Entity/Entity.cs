using System;
using System.Diagnostics.CodeAnalysis;
using Godot;
using SevenGame.Utility;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public sealed partial class Entity : CharacterBody3D {
    [Export] public Character? Character { 
        get => _character;
        private set => _character ??= value;
    }
    private Character? _character;

    [Export] public CharacterData? CharacterData {
        get => Character?.Data;
        private set => SetCharacter(value);
    }

    [Export] public CharacterCostume? CharacterCostume {
        get => Character?.CharacterCostume;
        private set => SetCostume(value);
    }

    
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


    [Export] private NodePath WeaponPath {
        get => _weaponPath;
        set {
            this.SetValueFromNode<IWeapon>(value, ref _weaponPath);

            IWeapon? weapon = Weapon;
            if ( weapon is not null ) {
                weapon.SetSkeleton(Armature);
                weapon.ReloadModel(Character?.IsLoaded ?? false);
            }
        }
    }
    private NodePath _weaponPath = new();

    public IWeapon? Weapon {
        get {
            this.TryGetNode(_weaponPath, out IWeapon weapon);
            return weapon;
        }
        private set {
            if ( value is not Node node ) {
                return;
            }
            _weaponPath = GetPathTo(node);
            
            IWeapon? weapon = Weapon;
            if ( weapon is not null ) {
                weapon.SetSkeleton(Armature);
                weapon.ReloadModel(Character?.IsLoaded ?? false);
            }
        }
    }



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

    
    public Skeleton3D? Armature => Character?.Armature;


    [Signal] public delegate void CharacterChangedEventHandler(CharacterData? newCharacter, CharacterData? oldCharacter);


    public Entity() : base() {
        CollisionLayer = 1 << 1;

        _behaviourManager ??= null !;
        _character ??= null !;
    }



    public void SetCharacter(CharacterData? data, CharacterCostume? costume = null) {
        if ( this.IsEditorGetSetter() ) return;

        CharacterData? oldData = CharacterData;
        if ( data == oldData ) return;

        LoadableExtensions.UpdateLoadable(ref _character)
            .WithConstructor(() => data?.Instantiate(this, costume))
            .OnLoadUnloadEvent(OnCharacterLoadedUnloaded)
            .WhenFinished(() => EmitSignal(SignalName.CharacterChanged, data!, oldData!))
            .Execute();
    }

    public void SetCostume(CharacterCostume? costume) =>
        Character?.SetCostume(costume);


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

    private void OnCharacterLoadedUnloaded(bool isLoaded) {
        Weapon?.SetSkeleton(Armature);
    }

    private void OnCharacterChanged(CharacterData? newCharacter, CharacterData? oldCharacter) {
        OnCharacterLoadedUnloaded(Character?.IsLoaded ?? false);
    }


    public override void _EnterTree() {
        base._EnterTree();
        
        CharacterChanged -= OnCharacterChanged;
        CharacterChanged += OnCharacterChanged;
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

    public override void _Notification(int what) {
        base._Notification(what);
        if (what == NotificationWMWindowFocusIn) {
            // NotificationWMWindowFocusIn is also called on Rebuilding the project;
            // Reconnect to signal on Recompile
            _EnterTree();
        }
    }

}
