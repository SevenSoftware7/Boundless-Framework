using Godot;
using Godot.Collections;


namespace LandlessSkies.Core;

[Tool]
public partial class MeshCharacterModel : CharacterModel {

	[ExportGroup("Dependencies")]
	[Export] public Skeleton3D? Skeleton;
	[ExportGroup("")]

	[Export] protected Node3D Model { get; private set; } = null!;



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



	protected override bool LoadModelImmediate() {
		if ( Costume is not MeshCharacterCostume meshCostume ) return false;
		if ( ! IsInsideTree() || GetParent() is null || Owner is null ) return false;

		if ( meshCostume.ModelScene?.Instantiate() is not Node3D model ) return false;

		Model = model;
		ParentToSkeleton();
		Model.Name = $"{nameof(CharacterCostume)} - {meshCostume.DisplayName}";

		return true;
	}
	protected override void UnloadModelImmediate() {
		Model?.UnparentAndQueueFree();
		Model = null!;
	}



	public override void _ValidateProperty(Dictionary property) {
		base._ValidateProperty(property);
		
		StringName name = property["name"].AsStringName();
		
		if ( name == PropertyName.Model ) {
			property["usage"] = (int)(property["usage"].As<PropertyUsageFlags>() | PropertyUsageFlags.ReadOnly);
		}
	}
}