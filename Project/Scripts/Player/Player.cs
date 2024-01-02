using System;
using System.Runtime.CompilerServices;
using Godot;

using Godot.Collections;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public sealed partial class Player : Node {
	private byte _playerId;
	private CameraController3D? _cameraController;


	public const byte MaxPlayers = 10;
	public static readonly Player?[] Players = new Player[MaxPlayers];



	[Export] public byte PlayerId {
		get => _playerId;
		private set => SetPlayerId(value);
	}


	[Export] public CameraController3D? CameraController {
		get => _cameraController ??= GetNodeOrNull<CameraController3D>(nameof(CameraController));
		private set => _cameraController = value;
	}

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
		// PlayerId is already set to 0 (default) or the PlayerId is already set to this Player.
		if ( _playerId != 0 || Players[0] == this ) return;

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

		if ( Entity is not null ) CameraController?.SetEntityAsSubject(Entity);
		if ( ControlDevice is not null ) CameraController?.HandleCamera(ControlDevice);

		if ( Entity is null || CameraController is null || ControlDevice is null ) return;

		Callable.From(SendInput).CallDeferred();

		void SendInput() {
			Entity.HandleInput(new(
				ControlDevice,
				CameraController,
				Entity
			));
		}
	}

	public override void _Ready() {
		base._Ready();

		FindFreeId();

		if ( Engine.IsEditorHint() ) return;
	}

	public override void _ExitTree() {
		base._ExitTree();
		if ( this.IsEditorExitTree() ) return;

		UnsetPlayerId();
	}


	public readonly struct InputInfo(ControlDevice controlDevice, CameraController3D cameraController, Entity entity) {
		public Entity Entity => entity;
		public ControlDevice ControlDevice => controlDevice;
		public CameraController3D CameraController => cameraController;


		public readonly void RawInputToGroundedMovement(out Basis camRotation, out Vector3 groundedMovement){
			Vector3 camRight = CameraController.AbsoluteRotation.X;
			Vector3 camUp = Entity.Transform.Basis.Y;
			Vector3 camForward = camUp.Cross(camRight).Normalized();
			camRotation = Basis.LookingAt(camForward, camUp);

			Vector2 moveInput = ControlDevice.GetVector(ControlDevice.MotionType.Move);
			groundedMovement = camRotation * new Vector3(moveInput.X, 0, moveInput.Y);
		}
		public readonly void RawInputToCameraRelativeMovement(out Basis camRotation, out Vector3 cameraRelativeMovement){
			camRotation = CameraController.AbsoluteRotation;
			Vector2 moveInput = ControlDevice.GetVector(ControlDevice.MotionType.Move);
			cameraRelativeMovement = camRotation * new Vector3(moveInput.X, 0, moveInput.Y);
		}
	}
}