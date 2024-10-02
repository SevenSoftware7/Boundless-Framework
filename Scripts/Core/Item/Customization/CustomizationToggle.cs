namespace LandlessSkies.Core;

using System;
using Godot;

public readonly struct CustomizationToggle(string name, Action<bool> onUpdate) : ICustomization {
	public readonly string Name = name;

	private readonly Action<bool> OnUpdate = onUpdate;

	public Control? Build(HudPack hud) {
		Control? optionControl = hud.ToggleButton?.Instantiate<Control>();
		// optionControl.SetText(Name);

		// optionControl.Pressed += OnUpdate;

		return optionControl;
	}
}