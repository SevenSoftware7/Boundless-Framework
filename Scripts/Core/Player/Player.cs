namespace LandlessSkies.Core;

using Godot;
using SevenDev.Boundless.Utility;

[GlobalClass]
public sealed partial class Player : Node {
	public const byte MaxPlayers = 2;
	public static readonly Player?[] Players = new Player[MaxPlayers];


	[Export] public CameraController3D CameraController { get; private set; } = null!;
	[Export] public HudManager HudManager { get; private set; } = null!;

	[Export]
	public Entity? Entity {
		get;
		set {
			if (field == value) return;

			Entity? oldEntity = field;
			Callable.From(() => oldEntity?.PropagatePlayerDisavowing()).CallDeferred();

			Callable onKill = Callable.From<float>(OnEntityDeath);
			NodeExtensions.SwapSignalEmitter(ref field, value, Entity.SignalName.Death, onKill);
		}
	}

	public InputDevice InputDevice => InputManager.CurrentDevice; // TODO: actual Device Management



	public Player() : base() { }


	private void OnEntityDeath(float fromHealth) {
		GD.Print($"Entity {Entity?.Name} under the control of Player {Name} died from {fromHealth} Health.");
	}

	private sbyte GetPlayerId() => (sbyte)System.Array.FindIndex(Players, p => p == this);

	private bool SetPlayerId(byte value) {
		if (Players[value] != null) {
			return false;
		}

		UnsetPlayerId();
		Players[value] = this;
		return true;
	}

	private void UnsetPlayerId() {
		sbyte index = GetPlayerId();
		if (index == -1) return;

		Players[index] = null;
	}

	public override void _Process(double delta) {
		base._Process(delta);

		Entity?.PropagatePlayerHandling(this);
	}

	public override void _Ready() {
		base._Ready();

		int index = System.Array.FindIndex(Players, p => p is null);
		if (index == -1) {
			QueueFree();
			return;
		}
		Players[index] = this;

		// if (CameraController is not null && Entity is not null) {
		// 	CameraController.LocalRotation = Entity.GlobalBasis;
		// }
	}


	public override void _Notification(int what) {
		base._Notification(what);
		switch ((ulong)what) {
			case NotificationPredelete:
				UnsetPlayerId();
				break;
		}
	}
}