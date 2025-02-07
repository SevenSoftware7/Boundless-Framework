namespace LandlessSkies.Core;

using Godot;
using SevenDev.Boundless.Utility;

[Tool]
[GlobalClass]
public partial class BipedBehaviour : GroundedBehaviour {
	private float _moveSpeed;

	private Vector3 _lastDirection;

	private PromptControl? interactPrompt;
	private PointerControl? interactPointer;


	protected BipedBehaviour() : this(null) { }
	public BipedBehaviour(Entity? entity) : base(entity, new BipedJumpAction.Builder()) { }


	protected override void _Start(EntityBehaviour? previousBehaviour) {
		base._Start(previousBehaviour);

		interactPointer?.Enable();
	}
	protected override void _Stop(EntityBehaviour? nextBehaviour) {
		base._Stop(nextBehaviour);

		interactPrompt?.SetEnabled(false);
		interactPointer?.Disable();
	}


	public override void HandlePlayer(Player player) {
		base.HandlePlayer(player);

		if (!IsActive) return;

		if (interactPrompt is null && Entity.HudPack.InteractPrompt is not null) {
			interactPrompt ??= player.HudManager.AddPrompt(Entity.HudPack.InteractPrompt);
		}
		if (interactPointer is null && Entity.HudPack.InteractPointer is not null) {
			interactPointer ??= player.HudManager.AddPointer(Entity.HudPack.InteractPointer);
		}

		HandleInteraction(player);



		void HandleInteraction(Player player) {
			if (Entity is null) return;

			InteractTarget? target = InteractTarget.GetBestTarget(Entity, 3.25f);

			if (target is not null) {
				if (player.InputDevice.IsActionJustPressed(Inputs.Interact) && target.Interactable.IsInteractable(Entity)) {
					target.Interactable.Interact(Entity, player, target.ShapeIndex);
				}
			}


			if (interactPrompt is not null) {
				interactPrompt.Update(target);
				interactPrompt.SetKey(player.InputDevice.GetActionSymbol(Inputs.Interact));
			}

			if (interactPointer is not null) {
				interactPointer.Target = target?.Interactable.GlobalTransform;
			}
		}
	}

	public override void DisavowPlayer() {
		base.DisavowPlayer();

		interactPrompt?.Destroy();
		interactPrompt = null;

		interactPointer?.QueueFree();
		interactPointer = null;
	}

	protected override Vector3 ProcessGroundedMovement(double delta) {
		float floatDelta = (float)delta;

		// ---- Speed Calculation ----
		float newSpeed = Mathf.Min(_movement.Length(), Entity.Stats.SprintSpeed);

		float speedDelta = _moveSpeed < newSpeed ? Entity.Stats.Acceleration : Entity.Stats.Deceleration;
		_moveSpeed = _moveSpeed.MoveToward(newSpeed, speedDelta * floatDelta);

		// ----- Rotation & Movement -----
		float rotationSpeed = Entity.TraitModifiers.ApplyTo(Traits.GenericTurnSpeed, Entity.Stats.RotationSpeed);
		float finalSpeed = Entity.TraitModifiers.ApplyTo(Traits.GenericMoveSpeed, _moveSpeed);

		if (!_movement.IsZeroApprox()) {
			Vector3 normalizedInput = _movement.Normalized();

			_lastDirection = _lastDirection.Lerp(normalizedInput, rotationSpeed * floatDelta);
			Entity.GlobalForward = Entity.GlobalForward.SafeSlerp(normalizedInput, rotationSpeed * floatDelta);

			finalSpeed *= Mathf.Clamp(_lastDirection.Dot(Entity.GlobalForward), 0f, 1f);
		}


		Basis newRotation = Basis.LookingAt(Entity.GlobalForward, Entity.UpDirection);
		Entity.GlobalBasis = Entity.GlobalBasis.SafeSlerp(newRotation, (float)delta * rotationSpeed);

		return _lastDirection * finalSpeed;
	}

}