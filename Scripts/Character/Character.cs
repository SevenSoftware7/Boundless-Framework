using Godot;
using System;
using System.Linq;


namespace EndlessSkies.Core;

[Tool]
[GlobalClass]
public partial class Character : Model, IModelAttachment {

    [Export] public CharacterData Data { get; private set; }
    [Export] public Node3D Collisions { get; private set; }
    [Export] public Node3D Armature { get; private set; }
    [Export] public ModelProperties Properties { get; private set; }
    [Export] public CharacterModel CharacterModel { get; private set; }


    [Export] public CharacterCostume CharacterCostume {
        get => CharacterModel?.Costume;
        #if TOOLS
            private set => CallDeferred(MethodName.SetCostume, value);
            // Same as above.
        #else
            private set => SetCostume(value);
        #endif
    }

    public Node3D RootAttachment => this;
    public Skeleton3D Skeleton => Properties?.Skeleton;

    public Node3D GetAttachment(IModelAttachment.AttachmentPart key) =>
        Properties?.GetAttachment(key) ?? RootAttachment;



    public Character() : base() {;}
    public Character(IModelAttachment modelAttachment, CharacterData data) : base(modelAttachment) {
        Parent.AddChild(this);
        Owner = Parent.Owner;

        Name = nameof(Character);
        Data = data;
    }

    public void SetCostume(CharacterCostume costume) {
        if ( !IsNodeReady() || Engine.GetProcessFrames() == 0 ) return;
        if ( CharacterModel?.Costume == costume ) return;

        CharacterModel?.UnloadModel();
        CharacterModel?.QueueFree();
        CharacterModel = null;

        if ( costume == null ) return;

        CharacterModel = costume?.CreateModel(this);
        CharacterModel?.LoadModel();
    }



    protected override bool LoadModelImmediate() {
        if ( Parent == null ) return false;
        if ( Data == null ) return false;

        Collisions = Data.CollisionScene?.Instantiate() as Node3D;
        if ( Collisions != null ) {
            Collisions.Name = nameof(Collisions);

            Parent.AddChild(Collisions);
            Collisions.Owner = Parent.Owner;
        }

        Armature = Data.SkeletonScene?.Instantiate() as Node3D;
        if ( Armature != null ) {
            Properties = Armature.GetNodeOrNull<ModelProperties>(nameof(ModelProperties));
            Armature.Name = nameof(Armature);

            AddChild(Armature);
            Armature.Owner = Owner;
        }

        // CharacterModel?.LoadModel();

        return true;
    }

    protected override bool UnloadModelImmediate() {

        // CharacterModel?.UnloadModel();
        // CharacterModel?.QueueFree();
        // CharacterModel = null;

        Collisions?.QueueFree();
        Collisions = null;
        Armature?.QueueFree();
        Armature = null;
        Properties = null;

        return true;
    }
    // public override void _EnterTree() {
    //     base._EnterTree();
    //     if ( this.IsInvalidTreeCallback() ) return;

    //     #if TOOLS
    //         CallDeferred(MethodName.LoadModel);
    //     #else
    //         LoadModel();
    //     #endif
    // }

    // public override void _ExitTree() {
    //     base._ExitTree();
    //     if ( this.IsInvalidTreeCallback() ) return;

    //     UnloadModel();
    // }


}
