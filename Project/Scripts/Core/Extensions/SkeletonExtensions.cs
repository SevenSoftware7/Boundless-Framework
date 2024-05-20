namespace SevenDev.Utility;

using Godot;

public static class SkeletonExtensions {
	public static Vector3 GetBonePositionOrDefault(this Skeleton3D skeleton, StringName boneName, Vector3 defaultPosition) {
		if (skeleton is null)
			return defaultPosition;

		int boneIndex = skeleton.FindBone(boneName);
		if (boneIndex == -1)
			return defaultPosition;

		return skeleton.ToGlobal(skeleton.GetBoneGlobalPose(boneIndex).Origin);
	}
	public static bool TryGetBonePosition(this Skeleton3D skeleton, StringName boneName, out Vector3 position) {
		position = Vector3.Zero;
		if (skeleton is null)
			return false;

		int boneIndex = skeleton.FindBone(boneName);
		if (boneIndex == -1)
			return false;


		position = skeleton.ToGlobal(skeleton.GetBoneGlobalPose(boneIndex).Origin);
		return true;
	}


	public static Transform3D GetBoneTransformOrDefault(this Skeleton3D skeleton, StringName boneName, Transform3D defaultTransform) {
		if (skeleton is null)
			return defaultTransform;

		int boneIndex = skeleton.FindBone(boneName);
		if (boneIndex == -1)
			return defaultTransform;


		Transform3D pose = skeleton.GetBoneGlobalPose(boneIndex);
		return new Transform3D(skeleton.GlobalTransform.Basis * pose.Basis, skeleton.ToGlobal(pose.Origin));
	}
	public static bool TryGetBoneTransform(this Skeleton3D skeleton, StringName boneName, out Transform3D transform) {
		transform = Transform3D.Identity;
		if (skeleton is null)
			return false;

		int boneIndex = skeleton.FindBone(boneName);
		if (boneIndex == -1)
			return false;


		Transform3D pose = skeleton.GetBoneGlobalPose(boneIndex);
		transform = new Transform3D(skeleton.GlobalTransform.Basis * pose.Basis, skeleton.ToGlobal(pose.Origin));
		return true;
	}
}