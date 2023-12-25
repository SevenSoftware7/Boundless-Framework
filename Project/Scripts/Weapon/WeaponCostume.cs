using Godot;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public abstract partial class WeaponCostume : Resource, IUIObject {
	[Export] public string DisplayName { get; private set; } = "";
	[Export] public Texture2D? DisplayPortrait { get; private set; }

	public abstract WeaponModel Instantiate(Node3D root);
}