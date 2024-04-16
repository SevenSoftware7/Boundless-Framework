namespace LandlessSkies.Core;

using Godot;

public interface ISkeletonAdaptable : IHandAdaptable {
	void SetParentSkeleton(Skeleton3D? skeleton);
}