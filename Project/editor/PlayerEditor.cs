#if TOOLS

using Godot;
using Godot.Collections;


namespace LandlessSkies.Core;

public partial class Player {

    // private bool ExtendInspectorProperty(ExtendableInspector inspector, Variant.Type type, string name, PropertyHint hintType, string hintString, PropertyUsageFlags usageFlags, bool wide) {
    //     if (name == PropertyName.PlayerId) {
    //         EditorProperty splitContainer = new() {
    //             Label = "Player Id",
    //             Checkable = true
    //         };

    //         EditorSpinSlider playerIdSlider = new() {
    //             MinValue = 0,
    //             MaxValue = MaxPlayers - 1,
    //             Step = 1,
    //             Value = PlayerId,
    //         };
    //         playerIdSlider.ValueChanged += (newVal) => {
    //             PlayerId = (byte)newVal;
    //             NotifyPropertyListChanged();
    //         };

    //         splitContainer.AddChild(playerIdSlider);
    //         splitContainer.AddFocusable(playerIdSlider);
    //         inspector.AddCustomControl(splitContainer);
            
    //         return true;
    //     }
    //     return false;
    // }

    public override string[] _GetConfigurationWarnings() {
        string[] warnings = base._GetConfigurationWarnings();

        if ( Players[PlayerId] != this) {
            warnings ??= [];

            System.Array.Resize(ref warnings, warnings.Length + 1);
            warnings[^1] = $"PlayerId {PlayerId} is already in use.";
        }

        return warnings;
    }

    // TODO: wait for Godot to Implement a PropertyUsageFlags attribute to simplify this
    // example:
    // [Export] [PropertyUsageFlags((int)PropertyUsageFlags.Editor)] public Array<Player> PlayersList {
    //     get => new (Players);
    //     private set {;}
    // }
    public override Array<Dictionary> _GetPropertyList() {
        Array<Dictionary> defaultValue = base._GetPropertyList() ?? [];

        defaultValue.Add(
            new Dictionary() {
                { "name", "PlayersList" },
                { "type", (int)Variant.Type.Array },
                { "hint", (int)PropertyHint.None },
                { "hint_string", $"{Variant.Type.Object:D}/{PropertyHint.NodeType:D}:Player" },
                { "usage", (int)PropertyUsageFlags.Editor },
            }
        );

        defaultValue.Add(
            new Dictionary() {
                { "name", PropertyName.PlayerId },
                { "type", (int)Variant.Type.Int },
                { "hint", (int)PropertyHint.Range },
                { "hint_string", $"0,{MaxPlayers - 1}," },
                { "usage", (int)PropertyUsageFlags.Default },
            }
        );

        return defaultValue;
    }
    public override Variant _Get(StringName property) {
        if ( property == "PlayersList" ) {
            return new Array(Players);
        }
        if ( property == PropertyName.PlayerId ) {
            return PlayerId;
        }
        return base._Get(property);
    }

    public override bool _Set(StringName property, Variant value) {
        if ( property == PropertyName.PlayerId && value.VariantType == Variant.Type.Int ) {
            PlayerId = value.AsByte();
        }
        return base._Set(property, value);
    }

}

#endif