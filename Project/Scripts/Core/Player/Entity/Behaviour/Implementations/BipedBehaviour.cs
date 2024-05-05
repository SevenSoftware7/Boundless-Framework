namespace LandlessSkies.Core;

using Godot;
using SevenGame.Utility;

public partial class BipedBehaviour(Entity entity) : EntityBehaviour(entity) {
	private Vector3 _inputDirection;
	private float _moveSpeed;
	private MovementType _movementType;

	private Vector3 _lastDirection;

	private TimeDuration jumpBuffer = new(125);
	private TimeDuration coyoteTimer = new(150);
	private TimeDuration jumpCooldown = new(500);



	public override void Start(EntityBehaviour? previousBehaviour) {
		base.Start(previousBehaviour);

		Entity.MotionMode = CharacterBody3D.MotionModeEnum.Grounded;
	}


	public override void HandleInput(Entity entity, CameraController3D cameraController, InputDevice inputDevice) {
		base.HandleInput(entity, cameraController, inputDevice);

		if (inputDevice.IsActionPressed("jump")) {
			Jump();
		}

		Vector2 movement = inputDevice.GetVector("move_left", "move_right", "move_forward", "move_backward").ClampMagnitude(1f);
		cameraController.RawInputToGroundedMovement(Entity, movement, out _, out Vector3 groundedMovement);

		float speedSquared = groundedMovement.LengthSquared();
		MovementType speed = speedSquared switch {
			_ when Mathf.IsZeroApprox(speedSquared) => MovementType.Idle,
			_ when speedSquared <= 0.25f || inputDevice.IsActionPressed("walk") => MovementType.Walk,
			_ when inputDevice.IsActionPressed("evade") => MovementType.Sprint,
			_ => MovementType.Run
		};
		SetSpeed(speed);


		Move(groundedMovement.Normalized());
	}

	public override Interactable? GetInteractionCandidate() {
		return Interactable.GetNearestCandidate(Entity, 7.5f, 0.5f);
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
			float targetSpeed = fallSpeed;
			float fallInertia = verticalInertia.Dot(-Entity.UpDirection);

			// Float more if player is holding jump key & rising

			float isFloating = jumpBuffer.IsDone ? 0f : 1f;
			const float floatReductionFactor = 0.85f;
			float floatFactor = Mathf.Lerp(1f, floatReductionFactor, isFloating * Mathf.Clamp(1f - fallInertia, 0f, 1f));

			// Slightly ramp up inertia when falling

			const float fallIncreaseFactor = 1.75f;
			float inertiaRampFactor = Mathf.Lerp(1f, fallIncreaseFactor, Mathf.Clamp((1f + fallInertia) * 0.5f, 0f, 1f));

			Vector3 targetInertia = -Entity.UpDirection * Mathf.Max(targetSpeed, fallInertia);
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
			Entity.Inertia = Entity.Inertia.SlideOnFace(Entity.UpDirection) + Entity.UpDirection * 17.5f;
			jumpBuffer.End();
			jumpCooldown.Start();
		}
	}
}