namespace LandlessSkies.Core;

using System;
using Godot;

public struct CustomizationToggle(string name, Action<bool> onUpdate) : ICustomization {
	public readonly string Name = name;

	private readonly Action<bool> OnUpdate = onUpdate;

	public Control? Build(HudPack hud) {
		Control? optionControl = hud.ToggleButton?.Instantiate<Control>();
		// optionControl.SetText(Name);

		// optionControl.Pressed += OnUpdate;

		return optionControl;
	}

	public ICustomizationState State {
		readonly get => _state;
		set => _state = value switch {
			ToggleState state => state,
			_ => new ToggleState(false)
		};
	}
	private ToggleState _state = new(false);



	public readonly struct ToggleState(bool Value) : ICustomizationState {
		public readonly bool Value = Value;
	}
}