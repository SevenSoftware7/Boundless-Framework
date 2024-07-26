namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;
using static Godot.CharacterBody3D;

[Tool]
public abstract partial class GroundedBehaviour : MovementBehaviour, IPlayerHandler {
	protected JumpActionInfo? JumpAction { get; init; }
	protected Vector3 _jumpDirection;
	protected Vector3 _moveDirection;

	protected readonly TimeDuration jumpBuffer = new(125);
	protected readonly TimeDuration coyoteTimer = new(150);
	protected readonly TimeDuration jumpCooldown = new(500);


	protected GroundedBehaviour() : base() { }
	public GroundedBehaviour(Entity entity, JumpActionInfo? jumpAction = null) : base(entity) {
		JumpAction = jumpAction;
	}


	protected override void _Start(EntityBehaviour? previousBehaviour) {
		Entity.GlobalForward = Entity.GlobalForward.SlideOnFace(Entity.UpDirection).Normalized();
		Entity.GlobalBasis = Basis.LookingAt(Entity.GlobalForward, Entity.UpDirection);

		Entity.MotionMode = MotionModeEnum.Grounded;
	}
	protected override void _Stop() {
		DisavowPlayer();
	}


	public virtual void HandlePlayer(Player player) {
		if (player.InputDevice.IsActionJustPressed(Inputs.Jump)) {
			Jump(Entity.UpDirection);
		}

		Vector2 input = player.InputDevice.GetVector(
			Inputs.MoveLeft, Inputs.MoveRight,
			Inputs.MoveForward, Inputs.MoveBackward
		).ClampMagnitude(1f);
		player.CameraController.RawInputToGroundedMovement(Entity, input, out _, out Vector3 movement);


		Move(movement);
	}

	public virtual void DisavowPlayer() {
		_moveDirection = Vector3.Zero;
		_jumpDirection = Entity.UpDirection;
	}



	public virtual bool SetMovementType(MovementType speed) => true;
	public override bool Move(Vector3 direction) {
		_moveDirection = direction;
		return true;
	}
	public virtual bool Jump(Vector3 direction) {
		_jumpDirection = direction;
		jumpBuffer.Start();
		return true;
	}



	protected sealed override void HandleMovement(double delta) {
		HandleInertia(delta);

		if (
			Entity.IsOnFloor() && Entity.GetPlatformVelocity().IsZeroApprox() &&
			(Entity.RecoverLocationBuffer.Count == 0 || Entity.GlobalPosition.DistanceSquaredTo(Entity.RecoverLocationBuffer[^1]) > 0.1f)
		) {
			if (Entity.RecoverLocationBuffer.Count >= Entity.RECOVER_LOCATION_BUFFER_SIZE) {
				Entity.RecoverLocationBuffer.RemoveRange(0, Entity.RecoverLocationBuffer.Count - Entity.RECOVER_LOCATION_BUFFER_SIZE);
			}
			Entity.RecoverLocationBuffer.Add(Entity.GlobalPosition);
		}

		HandleGroundedMovement(delta);

		if (MoveStep(delta)) {
			Entity.Movement = Vector3.Zero;
		}

		HandleJump(delta);


		bool MoveStep(double delta) {
			if (Mathf.IsZeroApprox(Entity.Movement.LengthSquared()) || !Entity.IsOnFloor()) return false;

			Vector3 movement = Entity.Movement * (float)delta;
			Vector3 destination = Entity.GlobalTransform.Origin + movement;


			// Search for obstacle (step) where the entity is moving
			KinematicCollision3D? stepObstacleCollision = Entity.MoveAndCollide(movement, true);

			// Not a valid step if the obstacle's surface is not steep
			if (stepObstacleCollision is not null && Mathf.Abs(stepObstacleCollision.GetNormal().Dot(Entity.UpDirection)) >= 0.25f)
				return false;

			float margin = Mathf.Epsilon;

			Vector3 sweepStart = destination;
			Vector3 sweepMotion = (Entity.Stats.StepHeight + margin) * -Entity.UpDirection;

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
			// If the step was not the same collider as the obstacle, we stop here
			if (stepObstacleCollision is not null && stepTestResult.GetColliderRid() != stepObstacleCollision.GetColliderRid()) return false;


			Vector3 point = stepTestResult.GetCollisionPoint();

			Vector3 destinationHeight = destination.Project(Entity.UpDirection);
			Vector3 pointHeight = point.Project(Entity.UpDirection);

			float stepHeightSquared = destinationHeight.DistanceSquaredTo(pointHeight);
			if (stepHeightSquared >= sweepMotion.LengthSquared()) return false;


			Entity.GlobalTransform = Entity.GlobalTransform with { Origin = destination - destinationHeight + pointHeight };

			return true;
		}
	}


	private void HandleInertia(double delta) {
		Entity.Inertia.Split(Entity.UpDirection, out Vector3 verticalInertia, out Vector3 horizontalInertia);

		horizontalInertia = ProcessHorizontalInertia(delta, horizontalInertia);
		verticalInertia = ProcessVerticalInertia(delta, verticalInertia);

		Entity.Inertia = verticalInertia + horizontalInertia;
	}

	protected virtual Vector3 ProcessHorizontalInertia(double delta, Vector3 horizontalInertia) {
		if (horizontalInertia.IsEqualApprox(Vector3.Zero)) return horizontalInertia;

		return horizontalInertia.MoveToward(
			Vector3.Zero,
			(Entity.IsOnFloor() && ! coyoteTimer.IsDone
				? 25f
				: 0.5f) * (float)delta
		);
	}

	protected virtual Vector3 ProcessVerticalInertia(double delta, Vector3 verticalInertia) {
		if (Entity.IsOnCeiling()) {
			verticalInertia = verticalInertia.SlideOnFace(-Entity.UpDirection);
		}
		if (Entity.IsOnFloor()) {
			verticalInertia = verticalInertia.SlideOnFace(Entity.UpDirection);
			return verticalInertia;
		}

		float fallSpeed = Entity.AttributeModifiers.ApplyTo(Attributes.GenericGravity, 32f);

		float fallInertia = verticalInertia.Dot(-Entity.UpDirection);
		Vector3 targetInertia = -Entity.UpDirection * fallSpeed;

		if (verticalInertia.IsEqualApprox(targetInertia)) return verticalInertia;

		const float fallIncreaseFactor = 1.75f;

		// Slightly ramp up inertia when falling
		float inertiaRampFactor = Mathf.Lerp(1f, fallIncreaseFactor, ((1f + fallInertia) * 0.5f).Clamp01());


		return verticalInertia.MoveToward(targetInertia, 45f * inertiaRampFactor * (float)delta);
	}

	protected abstract void HandleGroundedMovement(double delta);


	protected virtual void HandleJump(double delta) {
		if (Entity.IsOnFloor()) {
			coyoteTimer.Start();
		}

		if (JumpAction is not null && !jumpBuffer.IsDone && jumpCooldown.IsDone && !coyoteTimer.IsDone) {
			if (Entity.ExecuteAction(new JumpActionBuilder(JumpAction, _jumpDirection))) {
				jumpBuffer.End();
				jumpCooldown.Start();
			}
		}
	}


	public enum MovementType {
		Idle = 0,
		Walk = 1,
		Run = 2,
		Sprint = 3
	}
}