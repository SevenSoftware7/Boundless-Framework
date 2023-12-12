using Godot;


namespace LandlessSkies.Core;

[Tool]
public sealed partial class MeshWeaponModel : WeaponModel {

    [Export] private MeshInstance3D? Model;

    [ExportGroup("Dependencies")]
    [Export]
    public Skeleton3D? Skeleton { get; private set; }
    public IWeapon.Handedness Handedness { get; private set; }



    private MeshWeaponModel() : base() {}
    public MeshWeaponModel(MeshWeaponCostume costume, Node3D root) : base(costume, root) {}



    protected override bool LoadModelImmediate() {
        if ( Costume is not MeshWeaponCostume meshCostume ) return false;
        if ( GetParent() is null || Owner is null ) return false;
        if ( Skeleton is null ) return false;

        if ( meshCostume.ModelScene?.Instantiate() is not MeshInstance3D model ) return false;
        
        Model = model.SetOwnerAndParentTo(this);
        Model.Name = nameof(WeaponModel);
        Model.Skeleton = Model.GetPathTo(Skeleton);

        return true;
    }

    protected override bool UnloadModelImmediate() {
        Model?.UnparentAndQueueFree();
        Model = null;

        return true;
    }

    public override void Inject(Skeleton3D? skeleton) {
        Skeleton = skeleton;
        ReloadModel(true);
    }

    public override void Inject(IWeapon.Handedness handedness) {
        Handedness = handedness;
    }



    public override void _Process(double delta) {
        base._Process(delta);

        string boneName = Handedness switch {
            IWeapon.Handedness.Left         => "LeftHand",
            IWeapon.Handedness.Right or _   => "RightHand",
        };

        if ( Skeleton is not null && Skeleton.TryGetBoneTransform(boneName, out Transform3D handTransform) ) {
            GlobalTransform = handTransform;
        }
    }
}