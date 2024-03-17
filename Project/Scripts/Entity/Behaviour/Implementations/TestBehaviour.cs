namespace LandlessSkies.Core;

using Godot;
using SevenGame.Utility;
using static LandlessSkies.Core.ControlDevice.InputType;

public partial class TestBehaviour(Entity entity) : EntityBehaviour(entity) {
	private Vector3 _moveDirection;
	private float _moveSpeed;
	private MovementSpeed _movementSpeed;

	private TimeDuration jumpBuffer = new(125);
	private TimeDuration coyoteTimer = new(150);
	private TimeDuration jumpCooldown = new(500);



	public override void Start(EntityBehaviour? previousBehaviour) {
		base.Start(previousBehaviour);

		Entity.MotionMode = CharacterBody3D.MotionModeEnum.Grounded;
	}


	public override void HandleInput(Player.InputInfo inputInfo) {
		base.HandleInput(inputInfo);

		if (inputInfo.ControlDevice.IsInputPressed(ControlDevice.InputType.Jump)) {
			Jump();
		}

		inputInfo.RawInputToGroundedMovement(inputInfo.ControlDevice.GetVector(ControlDevice.MotionType.Move), out _, out Vector3 groundedMovement);

		float speedSquared = groundedMovement.LengthSquared();
		MovementSpeed speed = speedSquared switch {
			_ when Mathf.IsZeroApprox(speedSquared) => MovementSpeed.Idle,
			_ when speedSquared <= 0.25f => MovementSpeed.Walk,
			_ when inputInfo.ControlDevice.IsInputPressed(Evade) => MovementSpeed.Sprint,
			_ => MovementSpeed.Run
		};
		SetSpeed(speed);


		Move(groundedMovement);
	}

	public override bool SetSpeed(MovementSpeed speed) {
		if (!base.SetSpeed(speed))
			return false;
		if (speed == _movementSpeed)
			return false;

		// if (Entity.IsOnFloor() && Entity.CurrentAction is not EvadeAction) {
		// 	if ( speed == MovementSpeed.Idle ) {
		// 		MovementStopAnimation();
		// 	} else if ( (int)speed > (int)_movementSpeed ) {
		// 		MovementStartAnimation(speed);
		// 	}
		// }


		_movementSpeed = speed;
		return true;
	}
	public override bool Move(Vector3 direction) {
		if (!base.Move(direction))
			return false;

		_moveDirection = direction;
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

		float floatDelta = (float) delta;


		// ----- Inertia Calculations -----

		Entity.SplitInertia(out Vector3 verticalInertia, out Vector3 horizontalInertia);

		if (Entity.IsOnFloor()) {
			coyoteTimer.Start();
			horizontalInertia = horizontalInertia.MoveToward(Vector3.Zero, 0.25f * floatDelta);
		} else {
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



		float newSpeed = 0f;
		if (!_moveDirection.IsZeroApprox()) {

			// ----- Rotation -----

			float directionLength = _moveDirection.Length();
			Vector3 normalizedDirection = Entity.Transform.Basis.Inverse() * _moveDirection / directionLength;

			Entity.RelativeForward = Entity.RelativeForward.SafeSlerp(normalizedDirection, Entity.CharacterRotationSpeed * floatDelta);

			Entity.Character?.RotateTowards(Basis.LookingAt(normalizedDirection, Vector3.Up), delta);

			// Vector3 groundedMovement = _moveDirection;
			// if (Entity.IsOnFloor()) {
			//     groundedMovement = Entity.UpDirection.FromToBasis(Entity.GetFloorNormal()) * groundedMovement;
			// }


			// Select the speed based on the movement type

			newSpeed = _movementSpeed switch {
				_ when Entity.Character is null => CharacterData.DEFAULT_BASE_SPEED,
				MovementSpeed.Walk => Entity.Character.Data.slowSpeed,
				MovementSpeed.Run => Entity.Character.Data.baseSpeed,
				MovementSpeed.Sprint => Entity.Character.Data.sprintSpeed,
				_ => newSpeed
			};
		}

		// ---- Speed Calculation ----

		float accelerationFactor = 1f / Mathf.Max(newSpeed, Mathf.Epsilon);  // Accelerate faster depending on how big the difference between current and target speeds is

		float slowingFactor = (_moveSpeed < newSpeed) ? 1f : 0.5f;           // Slow down faster than speeding up

		// Move towards the new speed with acceleration

		float speedDifference = Mathf.Abs(newSpeed - _moveSpeed);
		float speedDelta = speedDifference * accelerationFactor * slowingFactor;
		_moveSpeed = Mathf.MoveToward(_moveSpeed, newSpeed, Entity.CharacterAcceleration * speedDelta * floatDelta);

		// Update the movement vector based on the new speed and direction

		Entity.Movement = _moveDirection * _moveSpeed;

		// ----- Jump Instruction -----

		if (!jumpBuffer.IsDone && jumpCooldown.IsDone && !coyoteTimer.IsDone) {
			Entity.Inertia = Entity.Inertia.SlideOnFace(Entity.UpDirection) + Entity.UpDirection * 17.5f;
			jumpBuffer.End();
			jumpCooldown.Start();
		}
	}
}