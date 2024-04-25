namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public sealed partial class WeaponMeshCostume : WeaponCostume, IMeshCostume {
	public override string DisplayName => _displayName;
	[Export] private string _displayName = string.Empty;

	public override Texture2D? DisplayPortrait => _displayPortrait;
	[Export] private Texture2D? _displayPortrait;

	[Export] public PackedScene? ModelScene { get; private set; }



	public override BoneMeshModel Instantiate() {
		return new BoneMeshModel(this);
	}
}