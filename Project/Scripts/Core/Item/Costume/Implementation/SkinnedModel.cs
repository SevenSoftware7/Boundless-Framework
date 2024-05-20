namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public partial class SkinnedModel : Model, ISkeletonAdaptable {
	[Export] protected GeometryInstance3D Model { get; private set; } = null!;

	[ExportGroup("Dependencies")]
	[Export] public Skeleton3D? Skeleton;
	[Export] public Handedness Handedness { get; private set; }



	protected SkinnedModel() : base() { }



	private void ParentToSkeleton() {
		if (Model is null || Model is not MeshInstance3D meshInstance) return;

		if (Skeleton is null) {
			meshInstance.Skeleton = "..";
			return;
		}

		meshInstance.Skeleton = meshInstance.GetPathTo(Skeleton);
	}

	protected virtual void HandleSkeleton() {
		if (Skeleton is not null) {
			GlobalTransform = Skeleton.GlobalTransform;
		}
	}


	public override Aabb GetAabb() => Model.GetAabb();

	public void SetHandedness(Handedness handedness) {
		Handedness = handedness;
	}
	public void SetParentSkeleton(Skeleton3D? skeleton) {
		Skeleton = skeleton;
		ParentToSkeleton();
	}



	public override void _Process(double delta) {
		base._Process(delta);
		HandleSkeleton();
	}
}