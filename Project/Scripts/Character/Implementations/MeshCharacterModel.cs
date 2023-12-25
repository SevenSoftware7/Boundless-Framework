using Godot;


namespace LandlessSkies.Core;

[Tool]
public sealed partial class MeshCharacterModel : CharacterModel {
	[Export] private MeshInstance3D? Model;

	[ExportGroup("Dependencies")]
	[Export] public Skeleton3D? Skeleton;



	private MeshCharacterModel() : base() {}
	public MeshCharacterModel(MeshCharacterCostume costume, Node3D root) : base(costume, root) {}



	public override void Inject(Skeleton3D? skeleton) {
		Skeleton = skeleton;
	}

	protected override bool LoadModelImmediate() {
		if ( Costume is not MeshCharacterCostume meshCostume ) return false;
		if ( GetParent() is null || Owner is null ) return false;
		if ( Skeleton is null ) return false;

		if ( meshCostume.ModelScene?.Instantiate() is not MeshInstance3D model ) return false;

		Model = model.SetOwnerAndParentTo(this);
		Model.Name = nameof(CharacterModel);
		Model.Skeleton = Model.GetPathTo(Skeleton);

		return true;
	}
	protected override bool UnloadModelImmediate() {
		Model?.QueueFree();
		Model = null;

		return true;
	}


	public override void _Process(double delta) {
		base._Process(delta);

		if ( Skeleton is not null ) {
			Transform = new(Skeleton.Transform.Basis, Skeleton.Transform.Origin);
		}
	}
}