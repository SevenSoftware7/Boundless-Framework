using Godot;
using Godot.Collections;

namespace SevenDev.Utility;

public static class VariantUtility {
	public static Dictionary GenerateProperty(StringName name, Variant.Type type, PropertyUsageFlags usageFlags, PropertyHint hint = PropertyHint.None, StringName? hintString = default) {
		return new Dictionary() {
			{ "name", name },
			{ "type", (int)type },
			{ "usage", (int)usageFlags },
			{ "hint", (int)hint },
			{ "hint_string", hintString ?? string.Empty },
		};
	}
}