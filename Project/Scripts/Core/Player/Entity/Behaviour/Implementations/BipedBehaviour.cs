namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;

public partial class BipedBehaviour(Entity Entity) : EntityBehaviour(Entity) {
	private Vector3 _inputDirection;
	private float _moveSpeed;
	private MovementType _movementType;

	private Vector3 _lastDirection;

	private readonly TimeDuration jumpBuffer = new(125);
	private readonly TimeDuration coyoteTimer = new(150);
	private readonly TimeDuration jumpCooldown = new(500);

	private PromptControl? interactPrompt;
	private PointerControl? interactPointer;

	public override void Start(EntityBehaviour? previousBehaviour) {
		base.Start(previousBehaviour);

		Entity.MotionMode = CharacterBody3D.MotionModeEnum.Grounded;
	}


	public override void HandlePlayer(Player player) {
		base.HandlePlayer(player);

		if (player.InputDevice.IsActionPressed("jump")) {
			Jump();
		}

		Vector2 movement = player.InputDevice.GetVector("move_left", "move_right", "move_forward", "move_backward").ClampMagnitude(1f);
		player.CameraController.RawInputToGroundedMovement(Entity, movement, out _, out Vector3 groundedMovement);

		float speedSquared = groundedMovement.LengthSquared();
		MovementType speed = speedSquared switch {
			_ when Mathf.IsZeroApprox(speedSquared) => MovementType.Idle,
			_ when speedSquared <= 0.25f || player.InputDevice.IsActionPressed("walk") => MovementType.Walk,
			_ when player.InputDevice.IsActionPressed("evade") => MovementType.Sprint,
			_ => MovementType.Run
		};
		SetSpeed(speed);


		Move(groundedMovement.Normalized());
		HandleInteraction(player.InputDevice, player.HudManager);
	}

	private void HandleInteraction(InputDevice inputDevice, HudManager hud) {
		InteractTarget? target = InteractTarget.GetBestTarget(Entity, 3.25f);

		if (target is not null) {
			if (inputDevice.IsActionJustPressed("interact") && target.Interactable.IsInteractable(Entity)) {
				target.Interactable.Interact(Entity, target.ShapeIndex);
			}
		}


		interactPrompt ??= hud.AddPrompt(Entity.HudPack.InteractPrompt);
		if (interactPrompt is not null) {
			interactPrompt.Update(target);
			interactPrompt.SetKey(inputDevice.GetActionSymbol("interact"));
		}

		interactPointer ??= hud.AddPointer(Entity.HudPack.InteractPointer);
		if (interactPointer is not null) {
			interactPointer.Target = target?.Interactable.GlobalTransform;
		}
	}

	public override bool SetSpeed(MovementType speed) {
		if (!base.SetSpeed(speed))
			return false;
		if (speed == _movementType)
			return false;

		// if (Entity.IsOnFloor() && Entity.CurrentAction is not EvadeAction) {
		// 	if ( speed == MovementSpeed.Idle ) {
		// 		MovementStopAnimation();
		// 	}
		// 	else if ( (int)speed > (int)_movementSpeed ) {
		// 		MovementStartAnimation(speed);
		// 	}
		// }


		_movementType = speed;
		return true;
	}
	public override bool Move(Vector3 direction) {
		if (!base.Move(direction))
			return false;

		_inputDirection = direction;
		return true;
	}
	public override bool Jump(Vector3? target = null) {
		if (!base.Jump(target))
			return false;

		jumpBuffer.Start();
		return true;
	}



	public override void _Process(double delta) {
		base._Process(delta);
		float floatDelta = (float)delta;

		// ----- Inertia Calculations -----

		Entity.SplitInertia(out Vector3 verticalInertia, out Vector3 horizontalInertia);

		if (Entity.IsOnFloor()) {
			coyoteTimer.Start();
			horizontalInertia = horizontalInertia.MoveToward(Vector3.Zero, 0.25f * floatDelta);
		}
		else {
			const float fallSpeed = 32f;
			const float floatReductionFactor = 0.65f;
			const float fallIncreaseFactor = 1.75f;

			float fallInertia = verticalInertia.Dot(-Entity.UpDirection);

			// Float more if player is holding jump key & rising

			float isFloating = jumpBuffer.IsDone
				? 0f
				: (1f - fallInertia).Clamp01();
			float floatFactor = Mathf.Lerp(1f, floatReductionFactor, isFloating);

			// Slightly ramp up inertia when falling

			float inertiaRampFactor = Mathf.Lerp(1f, fallIncreaseFactor, ((1f + fallInertia) * 0.5f).Clamp01());

			Vector3 targetInertia = -Entity.UpDirection * Mathf.Max(fallSpeed, fallInertia);
			verticalInertia = verticalInertia.MoveToward(targetInertia, 45f * floatFactor * inertiaRampFactor * floatDelta);
		}

		Entity.Inertia = verticalInertia + horizontalInertia;

		// Select the speed based on the movement type
		float newSpeed = _movementType switch {
			MovementType.Walk => Entity.Stats.SlowSpeed,
			MovementType.Run => Entity.Stats.BaseSpeed,
			MovementType.Sprint => Entity.Stats.SprintSpeed,
			_ => 0f
		};
		newSpeed = Entity.GetModifiers(Attributes.GenericMoveSpeed).Apply(newSpeed);

		Basis newRotation = Basis.LookingAt(Entity.AbsoluteForward, Vector3.Up);
		Entity.GlobalBasis = Entity.GlobalBasis.SafeSlerp(newRotation, (float)delta * Entity.Stats.RotationSpeed);

		// ---- Speed Calculation ----

		float speedDelta = _moveSpeed < newSpeed ? Entity.Stats.Acceleration : Entity.Stats.Deceleration;
		_moveSpeed = Mathf.MoveToward(_moveSpeed, newSpeed, speedDelta * floatDelta);

		// ----- Rotation & Movement -----

		if (_movementType != MovementType.Idle) {
			Vector3 normalizedInput = _inputDirection.Normalized();

			_lastDirection = _lastDirection.Lerp(normalizedInput, Entity.Stats.RotationSpeed * floatDelta);
			Entity.AbsoluteForward = Entity.AbsoluteForward.SafeSlerp(normalizedInput, Entity.Stats.RotationSpeed * floatDelta);

			// Vector3 groundedMovement = _moveDirection;
			// if (Entity.IsOnFloor()) {
			//     groundedMovement = Entity.UpDirection.FromToBasis(Entity.GetFloorNormal()) * groundedMovement;
			// }
			Entity.Movement = _lastDirection * _moveSpeed * Mathf.Clamp(_lastDirection.Dot(Entity.AbsoluteForward), 0f, 1f);
		} else {
			Entity.Movement = _lastDirection * _moveSpeed;
		}

		// ----- Jump Instruction -----

		if (!jumpBuffer.IsDone && jumpCooldown.IsDone && !coyoteTimer.IsDone) {
			float jumpHeight = Entity.GetModifiers(Attributes.GenericjumpHeight).Apply(Entity.Stats.JumpHeight);

			Entity.Inertia = Entity.Inertia.SlideOnFace(Entity.UpDirection) + Entity.UpDirection * jumpHeight;
			jumpBuffer.End();
			jumpCooldown.Start();
		}
	}


	public override void _ExitTree() {
		base._ExitTree();
		interactPrompt?.QueueFree();
		interactPointer?.QueueFree();
	}
}