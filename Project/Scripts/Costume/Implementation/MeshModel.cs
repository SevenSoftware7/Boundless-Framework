namespace LandlessSkies.Core;

using Godot;

[Tool]
public partial class MeshModel : Model {

	[ExportGroup("Dependencies")]
	[Export] public Skeleton3D? Skeleton;
	[Export] public Handedness Handedness { get; private set; }
	[ExportGroup("")]

	[Export] protected Node3D Model { get; private set; } = null!;

	public override bool IsLoaded {
		get => _isLoaded;
		set {
			if (this.IsInitializationSetterCall()) {
				_isLoaded = value;
				return;
			}
			AsILoadable().LoadUnload(value);
		}
	}
	private bool _isLoaded = false;



	protected MeshModel() : base() { }
	public MeshModel(Costume costume) : base(costume) { }



	private void ParentToSkeleton() {
		if (Model is null)
			return;

		if (Skeleton is null) {
			Model.SafeReparentAndSetOwner(this);
			return;
		}

		Model.SafeReparentAndSetOwner(Skeleton);
	}


	public override void Inject(Skeleton3D? skeleton) {
		Skeleton = skeleton;
		ParentToSkeleton();
	}
	public override void Inject(Handedness handedness) {
		Handedness = handedness;
	}

	protected override bool LoadBehaviour() {
		if (!base.LoadBehaviour())
			return false;
		if (Costume is not IMeshCostume meshCostume)
			return false;

		if (meshCostume.ModelScene?.Instantiate() is not Node3D model)
			return false;

		Model = model;
		ParentToSkeleton();
		Model.ProcessMode = ProcessMode;
		Model.Visible = Visible;
		Model.Name = $"{nameof(Costume)} - {Costume.DisplayName}";

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