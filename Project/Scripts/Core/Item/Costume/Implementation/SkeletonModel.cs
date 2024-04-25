namespace LandlessSkies.Core;

using System.Linq;
using Godot;
using Godot.Collections;

[Tool]
public partial class SkeletonModel : Model {

	[ExportGroup("Dependencies")]
	[Export] public Handedness Handedness { get; private set; }
	[ExportGroup("")]

	[Export] protected Skeleton3D Model { get; private set; } = null!;

	public override bool IsLoaded {
		get => _isLoaded;
		set => this.BackingFieldLoadUnload(ref _isLoaded, value);
	}
	private bool _isLoaded;



	protected SkeletonModel() : base() { }
	public SkeletonModel(Costume costume) : base(costume) { }



	public override Aabb GetAabb() => _aabb;
	[Export] private Aabb _aabb;

	public void SetHandedness(Handedness handedness) {
		Handedness = handedness;
	}

	private void UpdateAabb() {
		_aabb = new();
		MergeChildrenAabb(Model);

		void MergeChildrenAabb(Node parent) {
			if (parent is GeometryInstance3D mesh) _aabb = _aabb.Merge(mesh.GetAabb());

			foreach (Node child in parent.GetChildren()) {
				MergeChildrenAabb(child);
			}
		}
	}

	protected override bool LoadBehaviour() {
		if (!base.LoadBehaviour())
			return false;
		if (Costume is not IMeshCostume meshCostume)
			return false;

		if (meshCostume.ModelScene?.Instantiate() is not Skeleton3D model)
			return false;

		Model = model.SafeReparentAndSetOwner(this);
		Model.ProcessMode = ProcessMode;
		Model.Visible = Visible;
		Model.Name = $"{nameof(Costume)} - {Costume.DisplayName}";

		UpdateAabb();

		_isLoaded = true;

		return true;
	}
	protected override void UnloadBehaviour() {
		base.UnloadBehaviour();
		Model?.UnparentAndQueueFree();
		Model = null!;

		_aabb = new();

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