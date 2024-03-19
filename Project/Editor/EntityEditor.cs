#if TOOLS

namespace LandlessSkies.Core;

using Godot;
using Godot.Collections;

public partial class Entity {
	public override void _ValidateProperty(Dictionary property) {
		base._ValidateProperty(property);

		StringName name = property["name"].AsStringName();


		if (name == PropertyName.Character) {
			property["usage"] = (int) (property["usage"].As<PropertyUsageFlags>() | PropertyUsageFlags.ReadOnly);


		} else if (
			name == PropertyName.CharacterCostume ||
			name == PropertyName.CharacterData
		) {
			property["usage"] = (int) (property["usage"].As<PropertyUsageFlags>() & ~PropertyUsageFlags.Storage);

		}
	}
}

#endif