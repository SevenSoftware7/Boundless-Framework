namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public partial class WeaponCostume : Costume {
	[Export] private string _displayName = string.Empty;
	public override string DisplayName => _displayName;

	[Export] private Texture2D? _displayPortrait = null;
	public override Texture2D? DisplayPortrait => _displayPortrait;
}