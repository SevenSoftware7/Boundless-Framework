namespace LandlessSkies.Core;

using Godot;

[Tool]
public partial class MeshModel : Model, ISkeletonAdaptable {
	[ExportGroup("Dependencies")]
	[Export] public Skeleton3D? Skeleton;
	[Export] public Handedness Handedness { get; private set; }
	[ExportGroup("")]

	[Export] protected GeometryInstance3D Model { get; private set; } = null!;

	public override bool IsLoaded {
		get => _isLoaded;
		set => this.BackingFieldLoadUnload(ref _isLoaded, value);
	}
	private bool _isLoaded;



	protected MeshModel() : base() { }
	public MeshModel(Costume costume) : base(costume) { }



	private void ParentToSkeleton() {
		if (Model is null || Model is not MeshInstance3D meshInstance)
			return;

		if (Skeleton is null) {
			// Model.SafeReparentAndSetOwner(this);
			meshInstance.Skeleton = "..";
			return;
		}

		meshInstance.Skeleton = meshInstance.GetPathTo(Skeleton);
		// Model.SafeReparentAndSetOwner(Skeleton);
	}


	public override Aabb GetAabb() => Model.GetAabb();

	public void SetHandedness(Handedness handedness) {
		Handedness = handedness;
	}
	public void SetParentSkeleton(Skeleton3D? skeleton) {
		Skeleton = skeleton;
		ParentToSkeleton();
	}

	protected override bool LoadBehaviour() {
		if (!base.LoadBehaviour())
			return false;
		if (Costume is not IMeshCostume meshCostume)
			return false;

		if (meshCostume.ModelScene?.Instantiate() is not GeometryInstance3D model)
			return false;

		Model = model.SetOwnerAndParent(this);
		Model.ProcessMode = ProcessMode;
		Model.Visible = Visible;
		Model.Name = $"{nameof(Costume)} - {Costume.DisplayName}";
		ParentToSkeleton();

		_isLoaded = true;

		return true;
	}
	protected override void UnloadBehaviour() {
		base.UnloadBehaviour();
		Model?.UnparentAndQueueFree();
		Model = null!;

		_isLoaded = false;
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
}