namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public partial class WeaponCostume : Costume {
	[Export] public PackedScene? ModelScene { get; private set; }

	[Export] public override string DisplayName { get; protected set; } = string.Empty;
	[Export] public override Texture2D? DisplayPortrait { get; protected set; }


	public override Model? Instantiate() => ModelScene?.Instantiate<Model>();
}