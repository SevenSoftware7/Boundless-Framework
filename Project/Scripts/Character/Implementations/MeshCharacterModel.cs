using Godot;
using Godot.Collections;


namespace LandlessSkies.Core;

[Tool]
public partial class MeshCharacterModel : CharacterModel {

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

			AsILoadable().SetLoaded(value);
		}
	}



	protected MeshCharacterModel() : base() {}
	public MeshCharacterModel(MeshCharacterCostume costume, Node3D root) : base(costume, root) {}



	public override void Inject(Skeleton3D? skeleton) {
		Skeleton = skeleton;
		ParentToSkeleton();
	}

	private void ParentToSkeleton() {
		if ( Model is null ) return;

		if (Skeleton is null) {
			Model.SafeReparentAndSetOwner(this);
			return;
		}

		Model.SafeReparentAndSetOwner(Skeleton);
	}



	protected override bool LoadModelBehaviour() {
		if ( ! base.LoadModelBehaviour() ) return false;
		if ( Costume is not MeshCharacterCostume meshCostume ) return false;

		if ( meshCostume.ModelScene?.Instantiate() is not Node3D model ) return false;

		Model = model;
		ParentToSkeleton();
		Model.Name = $"{nameof(CharacterCostume)} - {meshCostume.DisplayName}";

		_isLoaded = true;

		return true;
	}
	protected override void UnloadModelBehaviour() {
		base.UnloadModelBehaviour();
		Model?.UnparentAndQueueFree();
		Model = null!;

		_isLoaded = false;
	}



	public override void _ValidateProperty(Dictionary property) {
		base._ValidateProperty(property);
		
		StringName name = property["name"].AsStringName();
		
		if ( name == PropertyName.Model ) {
			property["usage"] = (int)(property["usage"].As<PropertyUsageFlags>() | PropertyUsageFlags.ReadOnly);
		}
	}
}