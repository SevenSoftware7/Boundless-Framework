namespace LandlessSkies.Core;

using System.Collections.Generic;
using System.Linq;


public struct MultiAttributeModifier(IEnumerable<IAttributeModifier>? attributes = null) : IAttributeModifier {
	public IEnumerable<IAttributeModifier> Attributes { get; private set; } = FlattenAttributes(attributes ?? []);
	public readonly bool IsStacking => true;

	private static IEnumerable<IAttributeModifier> FlattenAttributes(IEnumerable<IAttributeModifier> attributes) {
		foreach (IAttributeModifier discount in attributes) {
			if (discount is MultiAttributeModifier multi) {
				foreach (IAttributeModifier contained in multi.Attributes) yield return contained;
			}
			yield return discount;
		}
	}


	public readonly float ApplyTo(float baseValue) {
		float result = baseValue;

		IEnumerable<IAttributeModifier> stackingAttributes = Attributes.Where(a => a.IsStacking);
		foreach (IAttributeModifier attribute in stackingAttributes) {
			if (attribute is null) continue;

			result += attribute.ApplyTo(baseValue) - baseValue;
		}

		IEnumerable<IAttributeModifier> nonStackingAttributes = Attributes.Except(stackingAttributes);
		foreach (IAttributeModifier attribute in nonStackingAttributes) {
			if (attribute is null) continue;

			result = attribute.ApplyTo(result);
		}

		return result;
	}
}