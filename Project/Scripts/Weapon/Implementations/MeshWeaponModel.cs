using Godot;
using Godot.Collections;


namespace LandlessSkies.Core;

[Tool]
public sealed partial class MeshWeaponModel : WeaponModel {
	[Export] private MeshInstance3D? Model;

	[ExportGroup("Dependencies")]
	[Export] public Skeleton3D? Skeleton { get; private set; }
	public IWeapon.Handedness Handedness { get; private set; }



	private MeshWeaponModel() : base() {}
	public MeshWeaponModel(MeshWeaponCostume costume, Node3D root) : base(costume, root) {}



	protected override bool LoadModelImmediate() {
		if ( Costume is not MeshWeaponCostume meshCostume ) return false;
		if ( GetParent() is null || Owner is null ) return false;

		if ( meshCostume.ModelScene?.Instantiate() is not MeshInstance3D model ) return false;

		Model = model.SetOwnerAndParentTo(this);
		UpdateModelSkeleton();

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

		UpdateModelSkeleton();
	}
	public override void Inject(IWeapon.Handedness handedness) {
		Handedness = handedness;
	}

	private void UpdateModelSkeleton() {
		if ( Model is not null && Model.Owner == Skeleton?.Owner ) {
			Model.Skeleton = Model.GetPathTo(Skeleton ?? default);
		}
	}

	// public override void _Parented() {
	// 	base._Parented();

	// 	UpdateModelSkeleton();
	// }

	// public override void _PathRenamed() {
	// 	base._PathRenamed();

	// 	UpdateModelSkeleton();
	// }

	public override void _Process(double delta) {
		base._Process(delta);

		string boneName = Handedness switch {
			IWeapon.Handedness.Left         => "LeftHand",
			IWeapon.Handedness.Right or _   => "RightHand",
		};

		if ( Skeleton is not null && Skeleton.TryGetBoneTransform(boneName, out Transform3D handTransform) ) {
			GlobalTransform = handTransform;
		} else {
			Transform = Transform3D.Identity;
		}
	}

	public override void _ValidateProperty(Dictionary property) {
		base._ValidateProperty(property);
		
		StringName name = property["name"].AsStringName();
		
		if (
			name == PropertyName.Model
		) {
			property["usage"] = (int)(property["usage"].As<PropertyUsageFlags>() | PropertyUsageFlags.ReadOnly);
		}
	}
}