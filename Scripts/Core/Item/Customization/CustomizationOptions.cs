namespace LandlessSkies.Core;

using System;
using Godot;
using SevenDev.Boundless.Utility;

public readonly struct CustomizationOptions(string name, Span<string> options, Action<string> onUpdate) : ICustomization {
	public readonly string Name = name;

	private readonly string[] Options = [.. options];
	private readonly Action<string> OnUpdate = onUpdate;

	public Control? Build(HudPack hud) {
		if (hud.OptionList is null) return null;
		if (hud.OptionButton is null) return null;

		Control optionList = hud.OptionList.Instantiate<Control>();

		for (int i = 0; i < Options.Length; i++) {
			string option = Options[i];

			Control? optionControl = hud.OptionButton.Instantiate<Control>().ParentTo(optionList);
			// optionControl.SetText(option);

			// optionControl.Pressed += OnUpdate;
		}

		return optionList;
	}
}