namespace LandlessSkies.Core;

using Godot;

// TODO: when Interface reference [Export] is implemented in Godot, merge this with CharacterMeshModel and inherit from it
[Tool]
public partial class WeaponMeshModel : WeaponModel, ILoadable {

	#region Generic Mesh Model stuff
	[ExportGroup("Dependencies")]
	[Export] public Skeleton3D? Skeleton { get; private set; }
	public Handedness Handedness { get; private set; }
	[ExportGroup("")]

	[Export] protected Node3D Model { get; private set; } = null!;

	private bool _isLoaded = false;
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

	protected WeaponMeshModel() : base() { }
	public WeaponMeshModel(WeaponMeshCostume costume) : base(costume) { }




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

	protected override bool LoadModelBehaviour() {
		if (!base.LoadModelBehaviour())
			return false;
		if (Costume is not WeaponMeshCostume meshCostume)
			return false;

		if (meshCostume.ModelScene?.Instantiate() is not Node3D model)
			return false;

		Model = model;
		ParentToSkeleton();
		Model.SetProcess(IsProcessing());
		Model.Visible = Visible;
		Model.Name = $"{nameof(Costume)} - {meshCostume.DisplayName}";

		_isLoaded = true;

		return true;
	}
	protected override void UnloadModelBehaviour() {
		base.UnloadModelBehaviour();
		Model?.UnparentAndQueueFree();
		Model = null!;

		_isLoaded = false;
	}

	public override void Enable() {
		base.Enable();
		if (Model is not null) {
			Model.SetProcess(true);
			Model.Visible = true;
		}
	}
	public override void Disable() {
		base.Disable();
		if (Model is not null) {
			Model.SetProcess(false);
			Model.Visible = false;
		}
	}
	#endregion

	public override void _Process(double delta) {
		base._Process(delta);

		if (Model is null || !Model.IsInsideTree())
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