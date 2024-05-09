namespace LandlessSkies.Core;

using System.Collections.Generic;

public struct MultiAttributeModifier(IEnumerable<IAttributeModifier>? attributes = null) : IAttributeModifier {
	public IEnumerable<IAttributeModifier> Attributes { get; private set; } = attributes ?? [];
	public readonly float Apply(float baseValue) {
		float result = baseValue;

		foreach (IAttributeModifier attribute in Attributes) {
			if (attribute is null) continue;

			float operated = attribute.Apply(baseValue);
			result += operated - baseValue;
		}

		return result;
	}
}