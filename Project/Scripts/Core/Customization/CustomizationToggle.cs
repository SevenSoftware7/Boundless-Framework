namespace LandlessSkies.Core;

using System;

public readonly struct CustomizationToggle(Span<string> options, Action<string> onUpdate) : ICustomizationParameter {
	readonly string[] Options = [.. options];
	readonly Action<string> OnUpdate = onUpdate;

	public void Construct() {
		for (int i = 0; i < Options.Length; i++) {


		}
	}
}