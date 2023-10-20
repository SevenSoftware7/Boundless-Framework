using Godot;
using System;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class WeaponModel : Model {

    [Export] private MeshInstance3D Model;
    [Export] public WeaponCostume Costume { get; private set; }
    [Export(PropertyHint.NodePathValidTypes, nameof(Skeleton3D))] public NodePath SkeletonPath { get; private set; }



    private WeaponModel() : base() {;}
    public WeaponModel(Node3D root, Skeleton3D skeleton, WeaponCostume costume) : base(root) {
        Name = nameof(WeaponModel);

        SkeletonPath = GetPathTo(skeleton);
        
        Costume = costume;
    }



    protected override bool LoadModelImmediate() {
        if ( SkeletonPath == null ) return false;
        if ( Costume == null ) return false;

        Model = Costume.ModelScene?.Instantiate() as MeshInstance3D;
        if ( Model is not null ) {
            Model.Name = nameof(Model);

            this.AddChildSetOwner(Model);

            if ( this.TryGetNode(SkeletonPath, out Skeleton3D skeleton) ) {
                Model.Skeleton = Model.GetPathTo(skeleton);
            }
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

        // if ( Engine.IsEditorHint() ) return;

        if ( this.TryGetNode(SkeletonPath, out Skeleton3D skeleton) && skeleton.TryGetBoneTransform("RightHand", out Transform3D rightHandTransform) ) {
            GlobalTransform = rightHandTransform;
        }
    }
}
