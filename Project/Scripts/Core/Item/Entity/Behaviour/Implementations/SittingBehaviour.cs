namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;

public abstract partial class SittingBehaviour : EntityBehaviour, IPlayerHandler {
	private EntityBehaviour? previousBehaviour;
	private PromptControl? dismountPrompt;

	public virtual bool HasSetupPlayer => dismountPrompt is not null;
	protected abstract Transform3D SittingPosition { get; }


	protected SittingBehaviour() : this(null!) { }
	public SittingBehaviour(Entity entity) : base(entity) { }


	public override void Start(EntityBehaviour? previousBehaviour) {
		base.Start(previousBehaviour);
		this.previousBehaviour = previousBehaviour;
	}

	public override void Stop() {
		DisavowPlayer();

		QueueFree();
	}

	public void Dismount() {
		Entity.SetBehaviour(previousBehaviour);
	}

	public virtual void SetupPlayer(Player player) {
		dismountPrompt ??= player.HudManager.AddPrompt(player.Entity?.HudPack.InteractPrompt);
	}

	public virtual void HandlePlayer(Player player) {
		if (player.InputDevice.IsActionPressed("jump")) {
			Jump();
		}

		Vector2 movement = player.InputDevice.GetVector("move_left", "move_right", "move_forward", "move_backward").ClampMagnitude(1f);
		player.CameraController.RawInputToGroundedMovement(Entity, movement, out _, out Vector3 groundedMovement);


		Move(groundedMovement.Normalized());

		dismountPrompt?.Update(true, "Dismount", player.InputDevice.GetActionSymbol("interact"));
		if (player.InputDevice.IsActionJustPressed("interact")) {
			Dismount();
		}
	}

	public virtual void DisavowPlayer() {
		dismountPrompt?.SetText("Fuck Face");

		dismountPrompt?.QueueFree();
		dismountPrompt = null;
	}

	public override void _Process(double delta) {
		base._Process(delta);
		Entity.GlobalTransform = SittingPosition;
		Entity.GlobalForward = SittingPosition.Basis.Forward();
	}
}