namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;

public abstract partial class SittingBehaviour : EntityBehaviour, IPlayerHandler {
	private EntityBehaviour? previousBehaviour;
	private PromptControl? dismountPrompt;

	protected abstract Transform3D SittingPosition { get; }


	protected SittingBehaviour() : base() { }
	public SittingBehaviour(Entity entity) : base(entity) { }


	public override void Start(EntityBehaviour? previousBehaviour) {
		base.Start(previousBehaviour);
		this.previousBehaviour = previousBehaviour;
	}

	public override void Stop() {
		base.Stop();
		QueueFree();
	}

	public void Dismount() {
		Entity?.SetBehaviour(previousBehaviour);
	}


	public override void HandlePlayer(Player player) {
		base.HandlePlayer(player);
		dismountPrompt ??= player.HudManager.AddPrompt(player.Entity?.HudPack.InteractPrompt);

		dismountPrompt?.Update(true, "Dismount", player.InputDevice.GetActionSymbol(Inputs.Interact));
		if (player.InputDevice.IsActionJustPressed(Inputs.Interact)) {
			Dismount();
		}
	}

	public override void DisavowPlayer() {
		base.DisavowPlayer();
		dismountPrompt?.Destroy();
		dismountPrompt = null;
	}

	public override void _Process(double delta) {
		base._Process(delta);
		if (Entity is null) return;

		Entity.GlobalTransform = SittingPosition;
		Entity.GlobalForward = SittingPosition.Basis.Forward();
	}
}