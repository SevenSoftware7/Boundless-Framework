namespace Seven.Boundless;

using Godot;
using Seven.Boundless.Utility;

[Tool]
[GlobalClass]
public partial class BipedBehaviour : GroundedBehaviour {
	private Vector3 _targetDirection;
	private Vector3 _currentDirection;
	private float _targetSpeed;
	private float _currentSpeed;


	private PromptControl? interactPrompt;
	private PointerControl? interactPointer;

	protected BipedBehaviour() : this(null!) { }
	public BipedBehaviour(Entity entity) : base(entity, new BipedJumpAction.Builder()) { }


	protected override void _Start(EntityBehaviour? previousBehaviour) {
		base._Start(previousBehaviour);

		interactPointer?.Enable();
	}
	protected override void _ResetMovement(EntityBehaviour? previousBehaviour = null) {
		_currentSpeed = Entity.Movement.Length();
	}

	protected override void _Stop(EntityBehaviour? nextBehaviour) {
		base._Stop(nextBehaviour);

		_currentDirection = Vector3.Zero;
		_currentSpeed = 0f;

		interactPrompt?.SetEnabled(false);
		interactPointer?.Disable();
	}


	public override void Move(Vector3 movement, MovementType movementType = MovementType.Normal) {
		float speed = Entity.GetTraitValue(Traits.GenericMoveSpeed);
		_targetSpeed = movementType switch {
			MovementType.Slow => Entity.GetTraitValue(Traits.GenericSlowMoveSpeedMultiplier) * speed,
			MovementType.Fast => Entity.GetTraitValue(Traits.GenericFastMoveSpeedMultiplier) * speed,
			MovementType.Normal or _ => speed,
		};
		_targetDirection = movement.Normalized();
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

			if (target.HasValue) {
				if (player.InputDevice.IsActionJustPressed(Inputs.Interact) && target.Value.Interactable.IsInteractable(Entity)) {
					target.Value.Interactable.Interact(Entity, player, target.Value.ShapeIndex);
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

		float speedDelta = Entity.GetTraitValue(_currentSpeed < _targetSpeed ? Traits.GenericAcceleration : Traits.GenericDeceleration);
		_currentSpeed = _currentSpeed.MoveToward(_targetSpeed, speedDelta * floatDelta);

		// ----- Rotation & Movement -----
		float rotationSpeed = Entity.GetTraitValue(Traits.GenericTurnSpeed);

		Vector3 forward = Entity.GlobalBasis.Forward();
		if (!_targetDirection.IsZeroApprox()) {
			_currentDirection = _currentDirection.Lerp(_targetDirection, rotationSpeed * floatDelta);
			forward = _currentDirection.Normalized();

			_currentSpeed *= Mathf.Clamp(_targetDirection.Dot(forward) + 0.5f, 0f, 1f);
		}

		Basis rotation = Entity.GlobalBasis;
		Basis newRotation = BasisExtensions.UpTowards(rotation.Up(), forward);
		Entity.GlobalBasis = rotation = rotation.SafeSlerp(newRotation, rotationSpeed * floatDelta);

		_targetDirection = Vector3.Zero;
		_targetSpeed = 0f;
		return _currentDirection * _currentSpeed;
	}
}