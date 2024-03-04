using Godot;

namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public sealed partial class CharacterMeshCostume : CharacterCostume {
    public override string DisplayName => _displayName;
	[Export] private string _displayName = string.Empty;

	[Export] public PackedScene? ModelScene { get; private set; }

	public override CharacterMeshModel Instantiate() {
		return new CharacterMeshModel(this);
	}
}