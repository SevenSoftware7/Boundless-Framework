using Godot;


namespace LandlessSkies.Core;

[Tool]
public sealed partial class MeshWeaponModel : WeaponModel {

    [Export(PropertyHint.NodePathValidTypes, nameof(Skeleton3D))] public NodePath SkeletonPath { get; private set; } = new();
    [Export] private MeshInstance3D? Model;



    private MeshWeaponModel() : base() {}
    public MeshWeaponModel(MeshWeaponCostume costume) : base(costume) {}



    public override void SetSkeleton(Skeleton3D? skeleton) {
        SkeletonPath = skeleton is not null ? GetPathTo(skeleton) : new();
    }

    protected override bool LoadModelImmediate() {
        if ( Costume is null || Costume is not MeshWeaponCostume meshCostume ) return false;
        if ( ! this.TryGetNode(SkeletonPath, out Skeleton3D skeleton) ) return false;

        if ( meshCostume.ModelScene?.Instantiate() is not MeshInstance3D model ) return false;
        Model = model;

        this.AddChildAndSetOwner(Model);
        Model.Name = nameof(WeaponModel);

        Model.Skeleton = Model.GetPathTo(skeleton);

        return true;
    }

    protected override bool UnloadModelImmediate() {
        Model?.UnparentAndQueueFree();
        Model = null;

        return true;
    }


    public override void _Process(double delta) {
        base._Process(delta);

        if ( this.TryGetNode(SkeletonPath, out Skeleton3D skeleton) && skeleton.TryGetBoneTransform("RightHand", out Transform3D rightHandTransform) ) {
            GlobalTransform = rightHandTransform;
        }
    }
}