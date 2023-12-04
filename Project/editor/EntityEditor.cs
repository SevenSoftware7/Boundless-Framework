#if TOOLS

using Godot;
using Godot.Collections;


namespace LandlessSkies.Core;

public partial class Entity {

    public override Array<Dictionary> _GetPropertyList() {
        Array<Dictionary> defaultValue = base._GetPropertyList() ?? [];

        defaultValue.Add(
            new Dictionary() {
                { "name", PropertyName.WeaponPath },
                { "type", (int)Variant.Type.NodePath },
                { "hint", (int)PropertyHint.NodePathValidTypes },
                { "hint_string", IWeaponInfo.HintString },
                { "usage", (int)PropertyUsageFlags.Default },
            }
        );

        return defaultValue;
    }
    public override Variant _Get(StringName property) {
        if ( property == PropertyName.WeaponPath ) {
            return WeaponPath;
        }
        return base._Get(property);
    }

    public override bool _Set(StringName property, Variant value) {
        if ( property == PropertyName.WeaponPath && value.VariantType == Variant.Type.NodePath ) {
            WeaponPath = value.As<NodePath>();
        }
        return base._Set(property, value);
    }

}

#endif