namespace LandlessSkies.Core;

using Godot;

[Tool]
public partial class BoneMeshModel : MeshModel {

	protected BoneMeshModel() : base() { }
	public BoneMeshModel(Costume costume) : base(costume) { }



	public override void _Process(double delta) {
		base._Process(delta);

		if (Model is null || ! Model.IsInsideTree())
			return;

		string boneName = Handedness switch {
			Handedness.Left => "LeftHand",
			Handedness.Right or _ => "RightHand",
		};

		if (Skeleton is not null && Skeleton.TryGetBoneTransform(boneName, out Transform3D handTransform)) {
			Model.GlobalTransform = handTransform;
		} else {
			Model.Transform = Transform3D.Identity;
		}
	}
}