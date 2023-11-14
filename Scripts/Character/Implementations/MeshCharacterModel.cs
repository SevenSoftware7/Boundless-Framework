using Godot;


namespace LandlessSkies.Core;

[Tool]
public sealed partial class MeshCharacterModel : CharacterModel {


    [Export] private MeshInstance3D? Model;

    [ExportGroup("Dependencies")]
    [Export(PropertyHint.NodePathValidTypes, nameof(Skeleton3D))] public NodePath SkeletonPath { get; private set; } = new();



    private MeshCharacterModel() : base() {}
    public MeshCharacterModel(MeshCharacterCostume costume, Node3D root) : base(costume, root) {}



    public override void SetSkeleton(Skeleton3D? skeleton) {
        SkeletonPath = skeleton is not null ? GetPathTo(skeleton) : new();
    }

    protected override bool LoadModelImmediate() {
        if ( GetParent() is null || Owner is null ) return false;
        if ( Costume is not MeshCharacterCostume meshCostume ) return false;
        if ( SkeletonPath == null || ! this.TryGetNode(SkeletonPath, out Skeleton3D skeleton) ) return false;

        if ( meshCostume.ModelScene?.Instantiate() is not MeshInstance3D model ) return false;

        Model = model.SetOwnerAndParentTo(this);
        Model.Name = nameof(CharacterModel);
        Model.Skeleton = Model.GetPathTo(skeleton);

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