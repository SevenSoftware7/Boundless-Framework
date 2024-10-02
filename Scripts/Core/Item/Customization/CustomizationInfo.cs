namespace LandlessSkies.Core;

using System;

public readonly ref struct CustomizationInfo(ReadOnlySpan<ICustomization> Customizations, ReadOnlySpan<ICustomizable> SubCustomizables) {
	public ReadOnlySpan<ICustomization> Customizations { get; init; } = Customizations;
	public ReadOnlySpan<ICustomizable> SubCustomizables { get; init; } = SubCustomizables;
}