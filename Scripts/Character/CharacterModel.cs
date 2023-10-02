using Godot;
using System;


namespace EndlessSkies.Core;

[Tool]
[GlobalClass]
public partial class CharacterModel : Model {

    [Export] private MeshInstance3D Model;
    [Export] public CharacterCostume Costume { get; private set; }
    [Export(PropertyHint.NodePathValidTypes, nameof(Skeleton3D))] public NodePath SkeletonPath { get; private set; }



    protected CharacterModel() : base() {;}
    public CharacterModel(IModelAttachment modelAttachment, CharacterCostume costume) : base(modelAttachment) {
        SkeletonPath = modelAttachment.Skeleton.GetPath();

        Parent.AddChild(this);
        Owner = Parent.Owner;
        
        Name = nameof(CharacterModel);
        Costume = costume;
    }



    protected override bool LoadModelImmediate() {
        if ( SkeletonPath == null ) return false;
        if ( Costume == null ) return false;

        Model = Costume.ModelScene?.Instantiate() as MeshInstance3D;
        if ( Model != null ) {
            Model.Name = nameof(Model);

            Parent.AddChild(Model);
            Model.Owner = Parent.Owner;

            Model.Skeleton = SkeletonPath;
        }

        return true;
    }

    protected override bool UnloadModelImmediate() {
        Model?.QueueFree();
        Model = null;

        return true;
    }
}
