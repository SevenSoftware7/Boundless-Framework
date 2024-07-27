namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;

[Tool]
public abstract partial class SittingBehaviour : EntityBehaviour, IPlayerHandler {
	private EntityBehaviour? previousBehaviour;
	private PromptControl? dismountPrompt;

	protected abstract Transform3D SittingPosition { get; }
	protected sealed override bool IsOneTime => true;


	protected SittingBehaviour() : this(null!) { }
	public SittingBehaviour(Entity entity, EntityBehaviour? previousBehaviour = null) : base(entity) {
		this.previousBehaviour = previousBehaviour;
	}


	protected override void _Start(EntityBehaviour? previousBehaviour) {
		this.previousBehaviour ??= previousBehaviour;
	}
	protected override void _Stop(EntityBehaviour? nextBehaviour) {
		DisavowPlayer();
	}


	public virtual void Dismount() {
		Entity?.SetBehaviour(previousBehaviour);
	}


	public virtual void HandlePlayer(Player player) {
		if (!IsActive) return;
		dismountPrompt ??= player.HudManager.AddPrompt(player.Entity?.HudPack.InteractPrompt);

		dismountPrompt?.Update(true, "Dismount", player.InputDevice.GetActionSymbol(Inputs.Interact));
		if (player.InputDevice.IsActionJustPressed(Inputs.Interact)) {
			Dismount();
		}
	}

	public virtual void DisavowPlayer() {
		dismountPrompt?.Destroy();
		dismountPrompt = null;
	}

	public override void _Process(double delta) {
		base._Process(delta);

		if (Engine.IsEditorHint()) return;
		if (Entity is not Entity entity) return;

		entity.GlobalTransform = SittingPosition;
		entity.GlobalForward = SittingPosition.Basis.Forward();
	}
}