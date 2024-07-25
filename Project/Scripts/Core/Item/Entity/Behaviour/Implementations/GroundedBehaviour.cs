namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;
using static Godot.CharacterBody3D;


public abstract partial class GroundedBehaviour : MovementBehaviour, IPlayerHandler {
	protected JumpActionInfo? JumpAction { get; init; }
	protected Vector3 _inputDirection;

	protected readonly TimeDuration jumpBuffer = new(125);
	protected readonly TimeDuration coyoteTimer = new(150);
	protected readonly TimeDuration jumpCooldown = new(500);


	protected GroundedBehaviour() : base() { }
	public GroundedBehaviour(Entity entity) : base(entity) { }


	protected override void _Start(EntityBehaviour? previousBehaviour) {
		if (Entity is null) return;

		Entity.GlobalForward = Entity.GlobalForward.SlideOnFace(Entity.UpDirection).Normalized();
		Entity.GlobalBasis = Basis.LookingAt(Entity.GlobalForward, Entity.UpDirection);

		Entity.MotionMode = MotionModeEnum.Grounded;
	}
	protected override void _Stop() {
		DisavowPlayer();
	}


	public virtual void HandlePlayer(Player player) {
		if (Entity is null) return;

		if (player.InputDevice.IsActionJustPressed(Inputs.Jump)) {
			Jump();
		}

		Vector2 movement = player.InputDevice.GetVector(
			Inputs.MoveLeft, Inputs.MoveRight,
			Inputs.MoveForward, Inputs.MoveBackward
		).ClampMagnitude(1f);
		player.CameraController.RawInputToGroundedMovement(Entity, movement, out _, out Vector3 groundedMovement);


		Move(groundedMovement);
	}

	public virtual void DisavowPlayer() {
		_inputDirection = Vector3.Zero;
	}



	public virtual bool SetMovementType(MovementType speed) => true;
	public override bool Move(Vector3 direction) {
		if (!base.Move(direction)) return false;

		_inputDirection = direction;
		return true;
	}
	public virtual bool Jump(Vector3? target = null) {
		jumpBuffer.Start();
		return true;
	}



	protected override void HandleMovement(Entity entity, double delta) {
		HandleInertia(entity, delta);
		HandleWalk(entity, delta);
		HandleJump(entity, delta);
	}


	private void HandleInertia(Entity entity, double delta) {
		entity.Inertia.Split(entity.UpDirection, out Vector3 verticalInertia, out Vector3 horizontalInertia);

		horizontalInertia = ProcessHorizontalInertia(delta, horizontalInertia);
		verticalInertia = ProcessVerticalInertia(delta, verticalInertia);

		entity.Inertia = verticalInertia + horizontalInertia;
	}

	protected virtual Vector3 ProcessHorizontalInertia(double delta, Vector3 horizontalInertia) {
		if (Entity is null) return horizontalInertia;
		if (horizontalInertia.IsEqualApprox(Vector3.Zero)) return horizontalInertia;

		return horizontalInertia.MoveToward(
			Vector3.Zero,
			Entity.IsOnFloor()
				? 1.5f
				: 0.5f * (float)delta
		);
	}

	protected virtual Vector3 ProcessVerticalInertia(double delta, Vector3 verticalInertia) {
		if (Entity is null) return verticalInertia;
		if (Entity.IsOnFloor()) return verticalInertia.SlideOnFace(Entity.UpDirection);

		float fallSpeed = Entity.AttributeModifiers.ApplyTo(Attributes.GenericGravity, 32f);

		float fallInertia = verticalInertia.Dot(-Entity.UpDirection);
		Vector3 targetInertia = -Entity.UpDirection * fallSpeed;

		if (verticalInertia.IsEqualApprox(targetInertia)) return verticalInertia;

		const float fallIncreaseFactor = 1.75f;

		// Slightly ramp up inertia when falling
		float inertiaRampFactor = Mathf.Lerp(1f, fallIncreaseFactor, ((1f + fallInertia) * 0.5f).Clamp01());


		return verticalInertia.MoveToward(targetInertia, 45f * inertiaRampFactor * (float)delta);
	}


	protected virtual void HandleWalk(Entity entity, double delta) {
		if (
			entity.IsOnFloor() && entity.GetPlatformVelocity().IsZeroApprox() &&
			(entity.RecoverLocationBuffer.Count == 0 || entity.GlobalPosition.DistanceSquaredTo(entity.RecoverLocationBuffer[^1]) > 0.1f)
		) {
			if (entity.RecoverLocationBuffer.Count >= Entity.RECOVER_LOCATION_BUFFER_SIZE) entity.RecoverLocationBuffer.RemoveRange(0, entity.RecoverLocationBuffer.Count - Entity.RECOVER_LOCATION_BUFFER_SIZE);
			entity.RecoverLocationBuffer.Add(entity.GlobalPosition);
		}

		entity.Inertia.Split(entity.UpDirection, out Vector3 verticalInertia, out Vector3 horizontalInertia);

		if (entity.IsOnFloor()) {
			verticalInertia = verticalInertia.SlideOnFace(entity.UpDirection);
		}
		if (entity.IsOnCeiling()) {
			verticalInertia = verticalInertia.SlideOnFace(-entity.UpDirection);
		}

		entity.Inertia = horizontalInertia + verticalInertia;

		if (MoveStep(delta)) {
			entity.Movement = Vector3.Zero;
		}


		bool MoveStep(double delta) {
			if (Mathf.IsZeroApprox(entity.Movement.LengthSquared()) || !entity.IsOnFloor()) return false;

			Vector3 movement = entity.Movement * (float)delta;
			Vector3 destination = entity.GlobalTransform.Origin + movement;


			// Search for obstacle (step) where the entity is moving
			KinematicCollision3D? stepObstacleCollision = entity.MoveAndCollide(movement, true);

			// Not a valid step if the obstacle's surface is not steep
			if (stepObstacleCollision is not null && Mathf.Abs(stepObstacleCollision.GetNormal().Dot(entity.UpDirection)) >= 0.25f)
				return false;

			float margin = Mathf.Epsilon;

			Vector3 sweepStart = destination;
			Vector3 sweepMotion = (entity.Stats.StepHeight + margin) * -entity.UpDirection;

			// Search above the obstacle to find a step upwards
			if (stepObstacleCollision is not null) {
				sweepStart -= sweepMotion;
			}

			// Try to collide with the step upwards or downwards
			PhysicsTestMotionResult3D stepTestResult = new();
			bool findStep = PhysicsServer3D.BodyTestMotion(
				entity.GetRid(),
				new() {
					From = entity.GlobalTransform with { Origin = sweepStart },
					Motion = sweepMotion,
				},
				stepTestResult
			);

			if (!findStep) return false;
			// If the step was not the same collider as the obstacle, we stop here
			if (stepObstacleCollision is not null && stepTestResult.GetColliderRid() != stepObstacleCollision.GetColliderRid()) return false;


			Vector3 point = stepTestResult.GetCollisionPoint();

			Vector3 destinationHeight = destination.Project(entity.UpDirection);
			Vector3 pointHeight = point.Project(entity.UpDirection);

			float stepHeightSquared = destinationHeight.DistanceSquaredTo(pointHeight);
			if (stepHeightSquared >= sweepMotion.LengthSquared()) return false;


			entity.GlobalTransform = entity.GlobalTransform with { Origin = destination - destinationHeight + pointHeight };

			return true;
		}
	}


	protected virtual void HandleJump(Entity entity, double delta) {
		if (entity.IsOnFloor()) {
			coyoteTimer.Start();
		}

		if (JumpAction is not null && !jumpBuffer.IsDone && jumpCooldown.IsDone && !coyoteTimer.IsDone) {
			if (entity.ExecuteAction(new JumpActionBuilder(JumpAction, entity.UpDirection))) {
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