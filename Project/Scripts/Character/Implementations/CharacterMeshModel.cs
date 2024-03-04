using Godot;
using Godot.Collections;


namespace LandlessSkies.Core;

[Tool]
// TODO: See WeaponMeshModel
public partial class CharacterMeshModel : CharacterModel {

	[ExportGroup("Dependencies")]
	[Export] public Skeleton3D? Skeleton;
	[ExportGroup("")]

	[Export] protected Node3D Model { get; private set; } = null!;

	private bool _isLoaded = false;
    public override bool IsLoaded {
		get => _isLoaded;
		set {
			if ( this.IsInitializationSetterCall() ) {
				_isLoaded = value;
				return;
			}

			AsILoadable().LoadUnload(value);
		}
	}



	protected CharacterMeshModel() : base() {}
	public CharacterMeshModel(CharacterMeshCostume costume) : base(costume) {}




	private void ParentToSkeleton() {
		if ( Model is null ) return;

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

	protected override bool LoadModelBehaviour() {
		if ( ! base.LoadModelBehaviour() ) return false;
		if ( Costume is not CharacterMeshCostume meshCostume ) return false;

		if ( meshCostume.ModelScene?.Instantiate() is not Node3D model ) return false;

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
}