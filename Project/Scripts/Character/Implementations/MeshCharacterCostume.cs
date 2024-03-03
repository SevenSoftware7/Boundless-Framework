using Godot;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public sealed partial class MeshCharacterCostume : CharacterCostume {
	[Export] public PackedScene? ModelScene { get; private set; }

	public override CharacterModel Instantiate() {
		return new MeshCharacterModel(this);
	}
}