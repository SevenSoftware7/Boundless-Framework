using Godot;

using Godot.Collections;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public sealed partial class Player : Node {

    public const byte MaxPlayers = 10;

    
    public static readonly Player?[] Players = new Player[MaxPlayers];

    public byte PlayerId {
        get => _playerId;
        private set => SetPlayerId(value);
    }
    private byte _playerId;



    [Export] public CameraController3D? CameraController { 
        get => _cameraController ??= GetNodeOrNull<CameraController3D>(nameof(CameraController));
        private set => _cameraController = value;
    }
    private CameraController3D? _cameraController;

    [Export] public Entity? Entity { get; private set; }

    [Export] public ControlDevice? ControlDevice { get; private set; }



    public Player() : base() {
        Name = nameof(Player);
    }



    private void SetPlayerId(byte value) {
        if ( this.IsEditorGetSetter() ) {
            _playerId = value;
            return;
        }
        
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

    public override void _Process(double delta) {
        base._Process(delta);

        if ( Engine.IsEditorHint() ) return;

        if ( Entity is not null) {
            CameraController?.SetEntityAsSubject(Entity);
        }
        if ( ControlDevice is not null) {
            CameraController?.HandleCamera(ControlDevice);
        }

        if ( Entity is null || CameraController is null || ControlDevice is null ) return;

        Entity.HandleInput(new(
            ControlDevice,
            CameraController,
            Entity
        ));
    }

    public override void _Ready() {
        base._Ready();
    }


    public override void _EnterTree() {
        if ( this.IsEditorEnterTree() ) return;
        base._EnterTree();

        FindFreeId();
    }

    public override void _ExitTree() {
        if ( this.IsEditorExitTree() ) return;
        base._ExitTree();
        
        UnsetPlayerId();
    }


    public readonly struct InputInfo {
        public readonly Entity Entity;
        public readonly ControlDevice ControlDevice;
        public readonly CameraController3D CameraController;


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
