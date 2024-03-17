namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public sealed partial class CharacterMeshCostume : CharacterCostume, IMeshCostume {
	public override string DisplayName => _displayName;
	[Export] private string _displayName = string.Empty;

	[Export] public PackedScene? ModelScene { get; private set; }

	public override MeshModel Instantiate() {
		return new MeshModel(this);
	}
}