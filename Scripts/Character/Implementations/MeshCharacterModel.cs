using Godot;


namespace LandlessSkies.Core;

[Tool]
public sealed partial class MeshCharacterModel : CharacterModel {

    [Export(PropertyHint.NodePathValidTypes, nameof(Skeleton3D))] public NodePath SkeletonPath { get; private set; } = new();

    [Export] private MeshInstance3D? Model;

    

    private MeshCharacterModel() : base() {}
    public MeshCharacterModel(Node3D? root, Skeleton3D? skeleton, MeshCharacterCostume costume) : base(root, skeleton, costume) {
        SkeletonPath = skeleton is not null ? GetPathTo(skeleton) : SkeletonPath;
    }



    protected override bool LoadModelImmediate() {
        if ( SkeletonPath == null ) return false;
        if ( Costume == null ) return false;
        if ( Costume is not MeshCharacterCostume meshCostume ) return false;

        if ( meshCostume.ModelScene?.Instantiate() is not MeshInstance3D model ) return false;
        Model = model;

        this.AddChildSetOwner(Model);
        Model.Name = nameof(CharacterModel);

        if ( this.TryGetNode(SkeletonPath, out Skeleton3D skeleton) ) {
            Model.Skeleton = Model.GetPathTo(skeleton);
        }

        return true;
    }

    protected override bool UnloadModelImmediate() {

        Model?.QueueFree();
        Model = null;

        return true;
    }


    public override void _Process(double delta) {
        base._Process(delta);

        if ( this.TryGetNode(SkeletonPath, out Skeleton3D skeleton) ) {
            Transform = new(skeleton.Transform.Basis, skeleton.Transform.Origin);
        }
    }
}