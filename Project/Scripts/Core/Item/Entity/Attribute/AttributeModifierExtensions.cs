namespace LandlessSkies.Core;

using System.Collections.Generic;
using System.Linq;
using Godot;

public static class AttributeModifierExtensions {

	public static float ApplyTo(this IEnumerable<AttributeModifier> modifiers, EntityAttribute target, float baseValue) {
		float result = baseValue;
		modifiers = modifiers.Distinct().Where(a => a is not null && a.Target == target);

		IEnumerable<IAttributeModifier> nonStackingAttributes = modifiers.Where(a => !a.IsStacking);
		foreach (IAttributeModifier attribute in nonStackingAttributes) {
			result = attribute.ApplyTo(result);
		}

		IEnumerable<IAttributeModifier> stackingAttributes = modifiers.Except(nonStackingAttributes);
		foreach (IAttributeModifier attribute in stackingAttributes) {
			result += attribute.ApplyTo(baseValue) - baseValue;
		}

		return result;
	}

}