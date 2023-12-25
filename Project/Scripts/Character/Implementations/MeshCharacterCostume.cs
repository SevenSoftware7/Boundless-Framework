using Godot;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public sealed partial class MeshCharacterCostume : CharacterCostume {
	[Export] public PackedScene? ModelScene { get; private set; }

	public override CharacterModel Instantiate(Node3D root) {
		return new MeshCharacterModel(this, root);
	}
}