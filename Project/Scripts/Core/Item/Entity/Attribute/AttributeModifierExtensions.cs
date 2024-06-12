namespace LandlessSkies.Core;

using System.Collections.Generic;
using System.Linq;
using Godot;

public static class AttributeModifierExtensions {

	public static MultiAttributeModifier Get(this IEnumerable<AttributeModifier> modifiers, StringName attributeName) {
		return new(modifiers.Where(a => a is not null && a.Name == attributeName));
	}

}