using Godot;
using Godot.Collections;

namespace SevenDev.Utility;

public static class VariantUtility {
	public static Dictionary GenerateProperty(Variant value, PropertyUsageFlags usageFlags, PropertyHint hint = PropertyHint.None, StringName? hintString = default) {
		return new Dictionary() {
			{ "name", value },
			{ "type", (int)value.VariantType },
			{ "usage", (int)usageFlags },
			{ "hint", (int)hint },
			{ "hint_string", hintString ?? string.Empty },
		};
	}
}