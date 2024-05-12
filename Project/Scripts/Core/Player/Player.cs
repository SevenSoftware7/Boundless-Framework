namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;


[Tool]
[GlobalClass]
public sealed partial class Player : Node {
	public const byte MaxPlayers = 2;
	public static readonly Player?[] Players = new Player[MaxPlayers];



	[Export]
	public byte PlayerId {
		get => _playerId;
		private set => SetPlayerId(value);
	}
	private byte _playerId;


	[Export] public CameraController3D CameraController { get; private set; } = null!;
	[Export] public Entity Entity { get; private set; } = null!;
	[Export] public HudManager HudManager { get; private set; } = null!;



	public Player() : base() { }



	private void SetPlayerId(byte value) {
		if (this.IsInitializationSetterCall()) {
			_playerId = value;
			return;
		}

		UnsetPlayerId();

		if (Players[value] == null) {
			Players[value] = this;
		}
		_playerId = value;

		UpdateConfigurationWarnings();
	}

	private void FindFreeId() {
		// PlayerId is already set to 0 (default) or the PlayerId is already set to this Player.

		if (_playerId != 0 || Players[0] == this)
			return;

		byte index = (byte)System.Array.FindIndex(Players, p => p is null);
		SetPlayerId(index);
	}

	private void UnsetPlayerId() {
		if (Players[_playerId] == this) {
			Players[_playerId] = null;
		}
	}

	public override void _Process(double delta) {
		base._Process(delta);

		if (Engine.IsEditorHint())
			return;

		if (Entity is null || CameraController is null)
			return;

		// TODO: actual Device Management
		Entity.PropagateCall(nameof(IInputHandler.HandleInput), [Entity, CameraController, InputManager.CurrentDevice, HudManager]);
	}

	public override void _Ready() {
		base._Ready();

		FindFreeId();

		if (Engine.IsEditorHint())
			return;
	}

	public override void _ExitTree() {
		base._ExitTree();
		if (this.IsEditorExitTree())
			return;

		UnsetPlayerId();
	}
}