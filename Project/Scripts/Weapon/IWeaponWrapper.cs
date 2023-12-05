using Godot;
using Godot.Collections;
using LandlessSkies.Core;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class IWeaponWrapper : Resource {
    private NodePath Path = new();

    public IWeapon? Get(Node root) => root.GetNodeOrNull(Path) as IWeapon;
    public void Set(Node root, IWeapon? val) {
        if (val is null) {
            Path = new();
            return;
        }

        if (val is Node node) {
            Path = root.GetPathTo(node);
        }
    }

    public override Array<Dictionary> _GetPropertyList() {
        return [new Dictionary() {
            { "name", PropertyName.Path },
            { "type", (int)Variant.Type.NodePath },
            { "hint", (int)PropertyHint.NodePathValidTypes },
            { "hint_string", IWeaponInfo.HintString },
            { "usage", (int)PropertyUsageFlags.Default },
        }];
    }

    public override Variant _Get(StringName property) {
        if (property == PropertyName.Path) {
            return Path;
        }
        return base._Get(property);
    }

    public override bool _Set(StringName property, Variant value) {
        if (property == PropertyName.Path && 
            value.VariantType == Variant.Type.NodePath &&
            value.As<NodePath>() is NodePath newPath
        ) {
            Path = newPath;
            return true;
        }
        return base._Set(property, value);
    }
}