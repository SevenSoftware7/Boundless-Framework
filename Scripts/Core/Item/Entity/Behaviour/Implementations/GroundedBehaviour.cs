namespace LandlessSkies.Core;

using Godot;
using SevenDev.Boundless.Utility;

[Tool]
public abstract partial class GroundedBehaviour : MovementBehaviour, IPlayerHandler {
	protected JumpAction.Builder? JumpAction { get; init; }

	protected Vector3 _gravityVelocity;
	protected Vector3 _jumpDirection;

	protected readonly Countdown jumpBuffer = new(125, false);
	protected readonly Countdown coyoteTimer = new(150, false);
	protected readonly Countdown jumpCooldown = new(200, false);

	protected sealed override CharacterBody3D.MotionModeEnum MotionMode => CharacterBody3D.MotionModeEnum.Grounded;


	protected GroundedBehaviour() : this(null!) { }
	public GroundedBehaviour(Entity entity, JumpAction.Builder? jumpAction = null) : base(entity) {
		JumpAction = jumpAction;
	}

	protected override void _Stop(EntityBehaviour? nextBehaviour) {
		base._Stop(nextBehaviour);

		_gravityVelocity = Vector3.Zero;

		DisavowPlayer();
	}

	public virtual void HandlePlayer(Player player) {
		if (!IsActive) return;

		if (player.InputDevice.IsActionJustPressed(Inputs.Jump)) {
			Jump();
		}

		Vector2 input = player.InputDevice.GetVector(
			Inputs.MoveLeft, Inputs.MoveRight,
			Inputs.MoveForward, Inputs.MoveBackward
		).ClampMagnitude(1f);

		if (player.CameraController.GetGroundedMovement(Entity.UpDirection, input, out _, out Vector3 movement)) {
			float speedSquared = movement.LengthSquared();
			MovementType type = speedSquared switch {
				_ when speedSquared.IsZeroApprox() => MovementType.Idle,
				_ when speedSquared <= 0.25f || player.InputDevice.IsActionPressed(Inputs.Walk) => MovementType.Slow,
				_ when player.InputDevice.IsActionPressed(Inputs.Evade) => MovementType.Fast,
				_ => MovementType.Normal,
			};

			Move(movement, type);
		}

		if (player.Entity == Entity && player.CameraController.SetOrAddBehaviour<GravitatedCameraBehaviour>(
			() => new(player.CameraController),
			out var cameraBehaviour))
		{
			cameraBehaviour.Subject = Entity;
			cameraBehaviour.MoveCamera(
				player.InputDevice.GetVector(Inputs.LookLeft, Inputs.LookRight, Inputs.LookDown, Inputs.LookUp) * player.InputDevice.Sensitivity
			);
		}
	}

	public virtual void DisavowPlayer() { }

	public virtual bool Jump(bool force = false, Vector3? direction = null) {
		_jumpDirection = direction ?? Entity.UpDirection;
		if (force) {
			ExecuteJump();
		}
		else {
			jumpBuffer.Start();
		}
		return true;
	}



	protected sealed override Vector3 ProcessMovement(double delta) {
		HandleGravityRotation(delta);

		if (
			Entity.IsOnFloor() && Entity.GetPlatformVelocity().IsZeroApprox() &&
			(!Entity.recoverState.HasValue || Entity.GlobalPosition.DistanceSquaredTo(Entity.recoverState.Value.Location) > 0.1f)
		) {
			Entity.UpdateRecoverState();
		}

		Entity.Movement += ProcessGroundedMovement(delta);

		if (MoveStep(delta)) {
			Entity.Movement = Vector3.Zero;
		}

		HandleJump(delta);

		return Entity.Movement;


		bool MoveStep(double delta) {
			if (Mathf.IsZeroApprox(Entity.Movement.LengthSquared()) || !Entity.IsOnFloor()) return false;

			Vector3 movement = Entity.Movement * (float)delta;
			Vector3 destination = Entity.GlobalTransform.Origin + movement;


			// Search for obstacle (step) where the entity is moving
			KinematicCollision3D? stepObstacleCollision = Entity.MoveAndCollide(movement, true);

			// Not a valid step if the obstacle's surface is not steep
			if (stepObstacleCollision is not null && Mathf.Abs(stepObstacleCollision.GetNormal().Dot(Entity.UpDirection)) >= Mathfs.RadToDot(Entity.FloorMaxAngle))
				return false;

			float margin = Mathf.Epsilon;

			Vector3 sweepStart = destination;
			Vector3 sweepMotion = (Entity.GetTraitValue(Traits.GenericStepHeight) + margin) * -Entity.UpDirection;

			// Search above the obstacle to find a step upwards
			if (stepObstacleCollision is not null) {
				sweepStart -= sweepMotion;
			}

			// Try to collide with the step upwards or downwards
			PhysicsTestMotionResult3D stepTestResult = new();
			bool findStep = PhysicsServer3D.BodyTestMotion(
				Entity.GetRid(),
				new() {
					From = Entity.GlobalTransform with { Origin = sweepStart },
					Motion = sweepMotion,
				},
				stepTestResult
			);

			if (!findStep) return false;

			Vector3 point = stepTestResult.GetCollisionPoint();

			Vector3 destinationHeight = destination.Project(Entity.UpDirection);
			Vector3 pointHeight = point.Project(Entity.UpDirection);

			float stepHeightSquared = destinationHeight.DistanceSquaredTo(pointHeight);
			if (stepHeightSquared >= sweepMotion.LengthSquared()) return false;


			Entity.GlobalTransform = Entity.GlobalTransform with { Origin = destination - destinationHeight + pointHeight };

			return true;
		}
	}
	protected abstract Vector3 ProcessGroundedMovement(double delta);

	private void HandleGravityRotation(double delta) {
		Basis newRotation = ProcessGravityRotation(delta);
		Entity.UpDirection = newRotation.Up();

		Basis targetRotation = Entity.GlobalBasis.Slerp(newRotation, 22f * (float)delta);
		Entity.GlobalBasis = targetRotation;
	}
	protected Basis ProcessGravityRotation(double delta) {
		Basis rotation = Entity.GlobalBasis.Orthonormalized();

		Vector3 newUp = ProcessUpDirection(delta).Normalized();

		return rotation.WarpUpTowards(newUp).Orthonormalized();
	}
	protected virtual Vector3 ProcessUpDirection(double delta) => Entity.UpDirection;


	protected override Vector3 ProcessInertia(double delta) {
		float floatDelta = (float)delta;
		Vector3 upDirection = Entity.UpDirection;

		if (Entity.IsOnCeiling()) {
			Entity.Gravity = Entity.Gravity.SlideOnFace(-upDirection);
			Entity.Inertia = Entity.Inertia.SlideOnFace(-upDirection);
		}

		if (Entity.IsOnFloor()) {
			_gravityVelocity = Vector3.Zero;

			Vector3 newGravity = Entity.Gravity.SlideOnFace(upDirection).MoveToward(Vector3.Zero, 25f * floatDelta);
			Vector3 newInertia = Entity.Inertia.SlideOnFace(upDirection);

			if (!coyoteTimer.IsCompleted) {
				newInertia = newInertia.MoveToward(Vector3.Zero, 25f * floatDelta);
			}

			Entity.Gravity = newGravity;
			Entity.Inertia = newInertia;
		}
		else {
			float fallSpeed = Entity.GetTraitValue(Traits.GenericGravity);

			float fallInertia = Entity.Gravity.Dot(-upDirection);
			Vector3 targetGravity = -upDirection * fallSpeed;

			// Slightly ramp up inertia when falling
			float accelerateSpeed = Mathf.Clamp(Mathf.Remap(fallInertia, -2f, 2f, 0.9f, 0.4f), 0.4f, 0.9f);
			// float accelerateSpeed = Mathf.Lerp(1.5f, 0.7f, Mathf.Remap(fallInertia, -2f, 2f, 0f, 1f).Clamp01());

			Entity.Gravity = Entity.Gravity.SmoothDamp(targetGravity, ref _gravityVelocity, accelerateSpeed, Mathf.Inf, floatDelta);
			Entity.Inertia = Entity.Inertia.MoveToward(Vector3.Zero, 0.5f * floatDelta);
		}


		return Entity.Gravity + Entity.Inertia;
	}


	protected virtual void HandleJump(double delta) {
		if (Entity.IsOnFloor()) {
			coyoteTimer.Start();
		}

		if (!jumpBuffer.IsCompleted && !coyoteTimer.IsCompleted) {
			ExecuteJump();
		}
	}

	protected void ExecuteJump() {
		if (JumpAction is null) return;

		if (jumpCooldown.IsCompleted && Entity.ExecuteAction(JumpAction) && Entity.CurrentAction is JumpAction jumpAction) {
			jumpAction.Direction = _jumpDirection;
			jumpBuffer.End();
			jumpCooldown.Start();
		}
	}
}