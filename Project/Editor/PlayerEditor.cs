#if TOOLS

using Godot;
using Godot.Collections;


namespace LandlessSkies.Core;

public partial class Player {
    [Export] public Array<Player?> PlayersList {
        get => [.. Players];
        private set {;}
    }



    public override string[] _GetConfigurationWarnings() {
        string[] warnings = base._GetConfigurationWarnings();

        if ( Players[PlayerId] != this) {
            warnings ??= [];

            System.Array.Resize(ref warnings, warnings.Length + 1);
            warnings[^1] = $"PlayerId {PlayerId} is already in use.";
        }

        return warnings;
    }


	public override void _ValidateProperty(Dictionary property) {
		base._ValidateProperty(property);
		
		StringName name = property["name"].AsStringName();
		
		if (
			name == PropertyName.PlayersList
		) {
			property["usage"] = (int)(property["usage"].As<PropertyUsageFlags>() & ~PropertyUsageFlags.Storage | PropertyUsageFlags.ReadOnly);
		
		} else if (
			name == PropertyName.PlayerId
		) {
			property["hint"] = (int)PropertyHint.Range;
			property["hint_string"] = $"0,{MaxPlayers - 1},";
		}
	}
}
#endif