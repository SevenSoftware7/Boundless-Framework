using Godot;

using Godot.Collections;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public sealed partial class Player : Node {

    public const byte MaxPlayers = 10;
    public const string PlayerIdRange = "0,9,"; // MaxPlayers - 1

    
    public static readonly Player[] Players = new Player[MaxPlayers];

    [Export(PropertyHint.Range, PlayerIdRange)] public byte PlayerId { 
        get => _playerId;
        #if TOOLS
            private set => CallDeferred(nameof(SetPlayerId), value);
        #else
            private set => SetPlayerId(value);
        #endif
    }
    private byte _playerId;


    [Export] public Entity Entity { get; private set; }
    
    [Export] public CameraController3D CameraController { 
        get => _cameraController ??= GetNodeOrNull<CameraController3D>(nameof(CameraController)) ?? new();
        private set => _cameraController = value;
    }
    private CameraController3D _cameraController;

    [Export] public ControlDevice ControlDevice { get; private set; }



    private Vector2 _cameraMovementInput;



    public Player() : base() {
        Name = nameof(Player);
    }


    private void SetPlayerId(byte value) {
        UnsetPlayerId();

        if ( Players[value] == null ) {
            Players[value] = this;
        }
        _playerId = value;

        UpdateConfigurationWarnings();
    }

    private void FindFreeId() {
        if ( _playerId != 0 || Players[0] == this ) return; // PlayerId is already set to 0 (default) or the PlayerId is already set to this Player.

        byte emptySpot = 0;
        bool found = false;
        for (byte i = 0; i < Players.Length; i++) {
            if ( Players[i] == null ) {
                emptySpot = i;
                found = true;
                break;
            }
        }
        if ( ! found ) {
            GD.PrintErr("No free PlayerId found.");
            return;
        }

        SetPlayerId(emptySpot);
    }

    private void UnsetPlayerId() {
        if ( Players[_playerId] == this ) {
            Players[_playerId] = null;
        }
    }


    #if TOOLS

    // TODO: wait for Godot to Implement a PropertyUsageFlags attribute to simplifly this
    // example:
    // [Export] [PropertyUsageFlags((int)PropertyUsageFlags.Editor)] public Array<Player> PlayersList {
    //     get => new (Players);
    //     private set {;}
    // }
    public override Array<Dictionary> _GetPropertyList() {
        Array<Dictionary> defaultValue = base._GetPropertyList() ?? new();

        defaultValue.Add(
            new Dictionary() {
                { "name", "PlayersList" },
                { "type", (int)Variant.Type.Array },
                { "hint", (int)PropertyHint.None },
                { "hint_string", $"{Variant.Type.Object:D}/{PropertyHint.NodeType:D}:Player" },
                { "usage", (int)PropertyUsageFlags.Editor },
            }
        );

        return defaultValue;
    }
    public override Variant _Get(StringName property) {
        if ( property == "PlayersList" ) {
            return new Array(Players);
        }
        return base._Get(property);
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

    #endif

    // public override void _Input(InputEvent @event) {
    //     base._Input(@event);

    //     // get mouse input
    //     if ( @event is InputEventMouseMotion mouseMotion ) {
    //         _cameraMovementInput = new(
    //             mouseMotion.Relative.X,
    //             -mouseMotion.Relative.Y
    //         );
    //     }
    // }

    public override void _Process(double delta) {
        base._Process(delta);

        if ( Engine.IsEditorHint() ) {

        } else {

            if (CameraController is not null) {
                CameraController.SubjectBasis = Entity.Transform.Basis;
                
                Vector3 camAnchor = Entity.Skeleton.GetBonePositionOrDefault("Head", Entity.Transform.Origin);
                CameraController.Subject = camAnchor;

                CameraController.HandleCameraInput(ControlDevice.GetLookDirection() * 0.005f);
            }

            Entity.HandleInput(new(
                ControlDevice,
                CameraController,
                Entity
            ));
        }
    }

    public override void _Ready() {
        base._Ready();
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


    public struct InputInfo {
        public ControlDevice ControlDevice;
        private CameraController3D CameraController;
        private Entity Entity;


        public InputInfo(ControlDevice controlDevice, CameraController3D cameraController, Entity entity) {
            ControlDevice = controlDevice;
            CameraController = cameraController;
            Entity = entity;
        }


        public readonly void RawInputToGroundedMovement(out Basis camRotation, out Vector3 groundedMovement){
            Vector3 camRight = CameraController.AbsoluteRotation.X;
            Vector3 camUp = Entity.Transform.Basis.Y;
            Vector3 camForward = camUp.Cross(camRight).Normalized();
            camRotation = Basis.LookingAt(camForward, camUp);

            Vector2 moveInput = ControlDevice.GetMoveDirection();
            groundedMovement = camRotation * new Vector3(moveInput.X, 0, moveInput.Y);
        }
        public readonly void RawInputToCameraRelativeMovement(out Basis camRotation, out Vector3 cameraRelativeMovement){
            camRotation = CameraController.AbsoluteRotation;
            Vector2 moveInput = ControlDevice.GetMoveDirection();
            cameraRelativeMovement = camRotation * new Vector3(moveInput.X, 0, moveInput.Y);
        }
    }
}
