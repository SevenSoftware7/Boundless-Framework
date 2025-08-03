namespace Seven.Boundless;

using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public sealed class CustomizationData(Dictionary<string, ICustomizationState> Customizations) {
	public Dictionary<string, ICustomizationState> Customizations { get; init; } = Customizations;

	public static CustomizationData GetFrom(ICustomizable customizable) {
		Dictionary<string, ICustomizationState> customizations = customizable.GetCustomizations()
			.ToDictionary(
				customization => customization.Key,
				customization => customization.Value.State
			);

		return new CustomizationData(customizations);
	}

	public void ApplyTo(ICustomizable customizable) {
		foreach (KeyValuePair<string, ICustomizationState> customization in Customizations) {
			if (customizable.GetCustomizations().TryGetValue(customization.Key, out ICustomization? customizationObject)) {
				customizationObject.State = customization.Value;
			}
		}
	}
}