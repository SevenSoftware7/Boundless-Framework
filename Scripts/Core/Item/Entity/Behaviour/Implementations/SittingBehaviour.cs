namespace LandlessSkies.Core;

using Godot;
using SevenDev.Boundless.Utility;

[Tool]
public abstract partial class SittingBehaviour : EntityBehaviour, IPlayerHandler {
	private EntityBehaviour? previousBehaviour;
	private PromptControl? dismountPrompt;

	protected abstract Transform3D SittingPosition { get; }
	protected override bool IsOneTime { get; } = true;


	protected SittingBehaviour() : this(null) { }
	public SittingBehaviour(Entity? entity, EntityBehaviour? previousBehaviour = null) : base(entity) {
		this.previousBehaviour = previousBehaviour;
	}


	protected override void _Start(EntityBehaviour? previousBehaviour) {
		this.previousBehaviour ??= previousBehaviour;
	}
	protected override void _Stop(EntityBehaviour? nextBehaviour) {
		DisavowPlayer();
	}


	public virtual void Dismount() {
		Entity.SetBehaviour(previousBehaviour);
	}


	public virtual void HandlePlayer(Player player) {
		if (!IsActive) return;

		if (dismountPrompt is null && Entity.HudPack.InteractPrompt is not null) {
			dismountPrompt = player.HudManager.AddPrompt(Entity.HudPack.InteractPrompt);
			dismountPrompt.Update(true, "Dismount");
		}
		dismountPrompt?.SetKey(player.InputDevice.GetActionSymbol(Inputs.Interact)); // TODO: update the key only when the InputDevice or the keybinding is changed

		if (player.InputDevice.IsActionJustPressed(Inputs.Interact)) {
			Dismount();
		}

		if (player.Entity == Entity && player.CameraController.SetOrAddBehaviour<GravitatedCameraBehaviour>(() => new(player.CameraController), out var cameraBehaviour)) {
			cameraBehaviour.SetEntityAsSubject(Entity);
			cameraBehaviour.MoveCamera(
				player.InputDevice.GetVector(Inputs.LookLeft, Inputs.LookRight, Inputs.LookDown, Inputs.LookUp) * player.InputDevice.Sensitivity
			);
		}
	}

	public virtual void DisavowPlayer() {
		dismountPrompt?.Destroy();
		dismountPrompt = null;
	}

	public override void _Process(double delta) {
		base._Process(delta);

		if (!IsActive) return;

		Entity.GlobalTransform = SittingPosition;
		Entity.GlobalForward = SittingPosition.Basis.Forward();
	}
}