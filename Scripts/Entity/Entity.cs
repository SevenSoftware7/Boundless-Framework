using Godot;
using Godot.Collections;
using System;
using System.Reflection.PortableExecutable;


namespace EndlessSkies.Core;

[Tool]
[GlobalClass]
public sealed partial class Entity : CharacterBody3D, IModelAttachment {

    [Export] public Vector3 Inertia = Vector3.Zero; 
    
    [Export] public EntityBehaviourManager BehaviourManager { 
        get => _behaviourManager ??= GetNodeOrNull<EntityBehaviourManager>(nameof(EntityBehaviourManager)) ?? new(this);
        private set => _behaviourManager = value;
    }
    private EntityBehaviourManager _behaviourManager;

    [Export] public Character Character { get; private set; }
    [Export] public CharacterModel CharacterModel { get; private set; }


    [Export] public CharacterData CharacterData {
        get => Character?.Data;
        // private set {;}
        #if TOOLS
            private set => CallDeferred(MethodName.SetCharacter, value, null as CharacterCostume);
            // We use CallDeferred here to ensure that the CharacterModel is loaded before we set the costume, as to not create a new Model for nothing.
        #else
            private set => SetCharacter(value);
        #endif
    }

    [Export] public CharacterCostume CharacterCostume {
        get => CharacterModel?.Costume;
        // private set {;}
        #if TOOLS
            private set => CallDeferred(MethodName.SetCostume, value);
            // Same as above.
        #else
            private set => SetCostume(value);
        #endif
    }

    
    
    public Node3D RootAttachment => Character?.RootAttachment ?? this;
    public Skeleton3D Skeleton => Character?.Skeleton;

    public Node3D GetAttachment(IModelAttachment.AttachmentPart key) =>
        Character?.GetAttachment(key) ?? RootAttachment;



    public void SetCharacter(CharacterData data, CharacterCostume costume = null) {

        if ( !IsNodeReady() || Engine.GetProcessFrames() == 0 ) return;
        if ( CharacterData == data ) return;

        Character?.UnloadModel();
        CharacterModel?.QueueFree();
        CharacterModel = null;

        Character?.UnloadModel();
        Character?.QueueFree();
        Character = null;

        if ( data == null ) return;

        Character = data?.CreateCollisions(this);
        Character?.LoadModel();

        SetCostume(costume ?? data?.BaseCostume);
    }

    public void SetCostume(CharacterCostume costume) {

        if ( !IsNodeReady() || Engine.GetProcessFrames() == 0 ) return;
        if ( CharacterCostume == costume ) return;

        CharacterModel?.UnloadModel();
        CharacterModel?.QueueFree();
        CharacterModel = null;

        if ( costume == null ) return;

        CharacterModel = costume?.CreateModel(this);
        CharacterModel?.LoadModel();
    }

    public void HandleInput(Player.InputInfo inputInfo) {
        BehaviourManager.CurrentBehaviour?.HandleInput(inputInfo);
    }


    /// <summary>
    /// Apply all Inertia to the Entity.
    /// </summary>
    private bool ProcessInertia(out Vector3 verticalInertia, out Vector3 horizontalInertia, double delta) {

        float floatDelta = (float)delta;

        // verticalInertia = Vector3.Zero;
        // horizontalInertia = Vector3.Zero;

        // if (/* GravityMultiplier == 0f || Weight == 0f ||  */gravityDown == Vector3.zero) return false;

        verticalInertia = Inertia.Project( -UpDirection );
        horizontalInertia = Inertia - verticalInertia;

        Vector3 gravityForce = -UpDirection * 1f;

        verticalInertia = verticalInertia.MoveToward(gravityForce, 35f * floatDelta );
        horizontalInertia = horizontalInertia.MoveToward(Vector3.Zero, 0.25f * floatDelta );

        // if (onGround) {
        //     verticalInertia = verticalInertia.NullifyInDirection(gravityDown);
        // }
        
        Inertia = horizontalInertia + verticalInertia;
        return true;
    }


    public override void _Process(double delta) {
        base._Process(delta);

        if ( Engine.IsEditorHint() ) return;

        
        ProcessInertia(out _, out _, delta);

        Vector3 deltaInertia = Inertia * (float)delta;
        Velocity += deltaInertia;
        MoveAndSlide();
        Velocity -= deltaInertia;
    }

    public override void _Ready() {
        GD.Print("Ready");
        base._Ready();

        if ( Engine.IsEditorHint() ) return;

        BehaviourManager.SetBehaviour<TestBehaviour>();
	}

}
