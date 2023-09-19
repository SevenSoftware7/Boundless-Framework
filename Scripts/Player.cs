using Godot;

using Godot.Collections;


namespace EndlessSkies.Core;

[Tool]
[GlobalClass]
public sealed partial class Player : Node {

    public static readonly Player[] Players = new Player[10];

    [Export] public byte PlayerId { 
        get => _playerId;
        private set {
            UnsetPlayerId();

            if ( Players[value] == null ) {
                Players[value] = this;
            }
            _playerId = value;
            _playerIdSet = true;

            UpdateConfigurationWarnings();
        }
    }
    private byte _playerId;
    private bool _playerIdSet = false; // workaround for when a project with a Player is loaded
    
    #if TOOLS
    [Export] private Array<Player> PlayersList {
        get => new(Players);
        set {;}
    }
    #endif


    [Export] public Entity Entity { get; private set; }



    public Player() : base() {
        Name = nameof(Player);
    }



    private void FindFreeId() {
        if ( _playerIdSet ) return; // The PlayerId was set by Godot on scene load. Unnecessary to find a new one.

        byte emptySpot = 0;
        bool found = false;
        for (byte i = 0; i < Players.Length; i++) {
            if ( Players[i] == null ) {
                emptySpot = i;
                found = true;
                break;
            }
        }
        if ( !found ) {
            GD.PrintErr("No free PlayerId found.");
            return;
        }

        PlayerId = emptySpot;
    }

    private void UnsetPlayerId() {
        if ( Players[_playerId] == this ) {
            Players[_playerId] = null;
        }
        _playerIdSet = false;
    }



    public override string[] _GetConfigurationWarnings() {
        string[] warnings = base._GetConfigurationWarnings();

        if ( Players[PlayerId] != this) {
            warnings ??= System.Array.Empty<string>();

            System.Array.Resize(ref warnings, warnings.Length + 1);
            warnings[^1] = $"PlayerId {PlayerId} is already in use.";
        }

        return warnings;
    }


    public override void _EnterTree() {
        if ( this.IsInvalidTreeCallback() ) return;
        base._EnterTree();

        FindFreeId();
    }

    public override void _ExitTree() {
        if ( this.IsInvalidTreeCallback() ) return;
        base._ExitTree();
        
        UnsetPlayerId();
    }
}
