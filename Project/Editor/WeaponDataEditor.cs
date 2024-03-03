#if TOOLS

using Godot;
using Godot.Collections;

namespace LandlessSkies.Core;

public partial class WeaponData {
	public override void _ValidateProperty(Dictionary property) {
		base._ValidateProperty(property);

		StringName name = property["name"].AsStringName();

		if (
			name == PropertyName.Type && ! EditableType ||
			name == PropertyName.Usage && ! EditableUsage ||
			name == PropertyName.Size && ! EditableSize
		) {
			property["usage"] = (int)(property["usage"].As<PropertyUsageFlags>() | PropertyUsageFlags.ReadOnly);

		}
	}
}

#endif