namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;

[Tool]
[GlobalClass]
public partial class BipedBehaviour : GroundedBehaviour {
	private float _moveSpeed;
	private MovementType _movementType;

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

		float speedSquared = _moveDirection.LengthSquared();
		MovementType speed = speedSquared switch {
			_ when Mathf.IsZeroApprox(speedSquared) => MovementType.Idle,
			_ when speedSquared <= 0.25f || player.InputDevice.IsActionPressed(Inputs.Walk) => MovementType.Walk,
			_ when player.InputDevice.IsActionPressed(Inputs.Evade) => MovementType.Sprint,
			_ => MovementType.Run
		};
		SetMovementType(speed);


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

		_lastDirection = Vector3.Zero;
		_movementType = MovementType.Idle;
	}

	public override bool SetMovementType(MovementType speed) {
		if (!base.SetMovementType(speed)) return false;
		if (speed == _movementType) return false;

		_movementType = speed;
		return true;
	}

	protected override void HandleGroundedMovement(double delta) {
		float floatDelta = (float)delta;

		// ---- Speed Calculation ----
		float newSpeed = _movementType switch {
			MovementType.Walk => Entity.Stats.SlowSpeed,
			MovementType.Run => Entity.Stats.BaseSpeed,
			MovementType.Sprint => Entity.Stats.SprintSpeed,
			_ => 0f
		};
		newSpeed = Entity.AttributeModifiers.ApplyTo(Attributes.GenericMoveSpeed, newSpeed);

		float speedDelta = _moveSpeed < newSpeed ? Entity.Stats.Acceleration : Entity.Stats.Deceleration;
		_moveSpeed = Mathf.MoveToward(_moveSpeed, newSpeed, speedDelta * floatDelta);


		// ----- Rotation & Movement -----
		float rotationSpeed = Entity.AttributeModifiers.ApplyTo(Attributes.GenericTurnSpeed, Entity.Stats.RotationSpeed);

		if (_movementType != MovementType.Idle) {
			Vector3 normalizedInput = _moveDirection.Normalized();

			_lastDirection = _lastDirection.Lerp(normalizedInput, rotationSpeed * floatDelta);
			Entity.GlobalForward = Entity.GlobalForward.SafeSlerp(normalizedInput, rotationSpeed * floatDelta);

			Entity.Movement = _lastDirection * _moveSpeed * Mathf.Clamp(_lastDirection.Dot(Entity.GlobalForward), 0f, 1f);
		}
		else {
			Entity.Movement = _lastDirection * _moveSpeed;
		}


		Basis newRotation = Basis.LookingAt(Entity.GlobalForward, Entity.UpDirection);
		Entity.GlobalBasis = Entity.GlobalBasis.SafeSlerp(newRotation, (float)delta * rotationSpeed);
	}

}