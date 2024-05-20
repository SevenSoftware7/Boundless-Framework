#if TOOLS

namespace LandlessSkies.Core;

using Godot;
using Godot.Collections;

public partial class Entity {
	public override void _ValidateProperty(Dictionary property) {
		base._ValidateProperty(property);

		StringName name = property["name"].AsStringName();


		if (name == PropertyName.Forward) {
			property["usage"] = (int)(property["usage"].As<PropertyUsageFlags>() & ~PropertyUsageFlags.Storage);
		}
	}
}

#endif