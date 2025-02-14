#if TOOLS

namespace LandlessSkies.Core;

using System;
using Godot;
using Godot.Collections;

public partial class Entity {
	public override void _ValidateProperty(Dictionary property) {
		base._ValidateProperty(property);

		StringName name = property["name"].AsStringName();


		if (name == PropertyName.Forward) {
			property["usage"] = (long)(property["usage"].As<PropertyUsageFlags>() & ~PropertyUsageFlags.Storage);
		}
	}

	public static implicit operator Entity(TraitModifierCollection v) {
		throw new NotImplementedException();
	}

}

#endif