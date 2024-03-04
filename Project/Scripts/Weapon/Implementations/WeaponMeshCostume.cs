using Godot;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public sealed partial class WeaponMeshCostume : WeaponCostume {
    public override string DisplayName => _displayName;
	[Export] private string _displayName = string.Empty;

    public override Texture2D? DisplayPortrait => _displayPortrait;
	[Export] private Texture2D? _displayPortrait;

    [Export] public PackedScene? ModelScene { get; private set; }

	public override WeaponMeshModel Instantiate() {
		return new WeaponMeshModel(this);
	}
}