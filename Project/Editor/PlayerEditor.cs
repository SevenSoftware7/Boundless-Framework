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
		
		switch (property["name"].AsStringName()) {
			case nameof(PlayerId):
				property["hint"] = (int)PropertyHint.Range;
				property["hint_string"] = $"0,{MaxPlayers - 1},";
				break;
			case nameof(PlayersList):
				property["usage"] = (int)(PropertyUsageFlags.Editor | PropertyUsageFlags.ReadOnly);
				break;
		}
	}
}
#endif