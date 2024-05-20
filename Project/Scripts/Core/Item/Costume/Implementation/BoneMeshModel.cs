namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;


[Tool]
[GlobalClass]
public partial class BoneMeshModel : MeshModel {

	protected BoneMeshModel() : base() { }


	protected override void HandleSkeleton() {
		if (Skeleton is not null) {
			string boneName = Handedness switch {
				Handedness.Left => "LeftHand",
				Handedness.Right or _ => "RightHand",
			};

			GlobalTransform = Skeleton.TryGetBoneTransform(boneName, out Transform3D handTransform)
				? handTransform
				: Skeleton.GlobalTransform;
		}
		else {
			Transform = Transform3D.Identity;
		}
	}
}