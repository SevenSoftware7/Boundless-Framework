namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public partial class MeshModel : Model, ISkeletonAdaptable {
	[ExportGroup("Dependencies")]
	[Export] public Skeleton3D? Skeleton;
	[Export] public Handedness Handedness { get; private set; }
	[ExportGroup("")]

	[Export] protected GeometryInstance3D Model { get; private set; } = null!;



	protected MeshModel() : base() { }



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

	protected override void EnableBehaviour() {
		base.EnableBehaviour();
		if (Model is not null) {
			Model.ProcessMode = ProcessModeEnum.Inherit;
			Model.Visible = true;
		}
	}
	protected override void DisableBehaviour() {
		base.DisableBehaviour();
		if (Model is not null) {
			Model.ProcessMode = ProcessModeEnum.Disabled;
			Model.Visible = false;
		}
	}

	public override void _Process(double delta) {
		base._Process(delta);
		HandleSkeleton();
	}
}