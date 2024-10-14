namespace LandlessSkies.Core;

using Godot;

public interface ICustomization {
	Control? Build(HudPack hud);

	ICustomizationState State { get; set; }
}