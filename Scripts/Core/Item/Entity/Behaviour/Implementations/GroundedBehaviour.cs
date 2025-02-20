namespace LandlessSkies.Core;

using Godot;
using SevenDev.Boundless.Utility;
using static Godot.CharacterBody3D;

[Tool]
public abstract partial class GroundedBehaviour : MovementBehaviour, IPlayerHandler {
	protected JumpAction.Builder? JumpAction { get; init; }

	protected Vector3 _jumpDirection;

	protected readonly TimeDuration jumpBuffer = new(125, false);
	protected readonly TimeDuration coyoteTimer = new(150, false);
	protected readonly TimeDuration jumpCooldown = new(200, false);


	protected GroundedBehaviour() : this(null) { }
	public GroundedBehaviour(Entity? entity, JumpAction.Builder? jumpAction = null) : base(entity) {
		JumpAction = jumpAction;
	}


	protected override void _Start(EntityBehaviour? previousBehaviour) {
		Entity.GlobalForward = Entity.GlobalForward.SlideOnFace(Entity.UpDirection).Normalized();
		Entity.GlobalBasis = Basis.LookingAt(Entity.GlobalForward, Entity.UpDirection);

		Entity.MotionMode = MotionModeEnum.Grounded;
	}
	protected override void _Stop(EntityBehaviour? nextBehaviour) {
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
		player.CameraController.GetGroundedMovement(Entity.UpDirection, input, out _, out Vector3 movement);

		float speedSquared = movement.LengthSquared();
		MovementType type = speedSquared switch {
			_ when speedSquared <= 0.25f || player.InputDevice.IsActionPressed(Inputs.Walk) => MovementType.Walk,
			_ when player.InputDevice.IsActionPressed(Inputs.Evade) => MovementType.Sprint,
			_ => MovementType.Run
		};

		Move(movement, type);


		if (player.Entity == Entity && player.CameraController.SetOrAddBehaviour<GravitatedCameraBehaviour>(
			() => new(player.CameraController),
			out var cameraBehaviour))
		{
			cameraBehaviour.SetEntityAsSubject(Entity);
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
		if (
			Entity.IsOnFloor() && Entity.GetPlatformVelocity().IsZeroApprox() &&
			(!Entity.LastRecoverLocation.HasValue || Entity.GlobalPosition.DistanceSquaredTo(Entity.LastRecoverLocation.Value) > 0.1f)
		) {
			Entity.AddRecoverLocation(Entity.GlobalPosition);
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
			// // If the step was not the same collider as the obstacle, we stop here
			// if (stepObstacleCollision is not null && stepTestResult.GetColliderRid() != stepObstacleCollision.GetColliderRid()) return false;


			Vector3 point = stepTestResult.GetCollisionPoint();

			Vector3 destinationHeight = destination.Project(Entity.UpDirection);
			Vector3 pointHeight = point.Project(Entity.UpDirection);

			float stepHeightSquared = destinationHeight.DistanceSquaredTo(pointHeight);
			if (stepHeightSquared >= sweepMotion.LengthSquared()) return false;


			Entity.GlobalTransform = Entity.GlobalTransform with { Origin = destination - destinationHeight + pointHeight };

			return true;
		}
	}


	protected override Vector3 ProcessInertia(double delta) {
		Vector3 upDirection = Entity.UpDirection;
		if (Entity.IsOnCeiling()) {
			Entity.Gravity = Entity.Gravity.SlideOnFace(-upDirection);
			Entity.Inertia = Entity.Inertia.SlideOnFace(-upDirection);
		}

		bool isOnFloor = Entity.IsOnFloor();

		if (isOnFloor) {
			Entity.Gravity = Entity.Gravity.SlideOnFace(upDirection);
			Entity.Inertia = Entity.Inertia.SlideOnFace(upDirection);

			Entity.Gravity = Entity.Gravity.MoveToward(Vector3.Zero, 25f * (float)delta);
			if (!coyoteTimer.HasElapsed) {
				Entity.Inertia = Entity.Inertia.MoveToward(Vector3.Zero, 25f * (float)delta);
			}
		}
		else {
			float fallSpeed = Entity.GetTraitValue(Traits.GenericGravity);

			float fallInertia = Entity.Gravity.Dot(-upDirection);
			Vector3 targetGravity = -upDirection * fallSpeed;

			const float fallIncreaseFactor = 1.75f;

			// Slightly ramp up inertia when falling
			float inertiaRampFactor = Mathf.Lerp(1f, fallIncreaseFactor, (fallInertia * 0.5f + 0.5f).Clamp01());

			Entity.Gravity = Entity.Gravity.MoveToward(targetGravity, 45f * inertiaRampFactor * (float)delta);
			Entity.Inertia = Entity.Inertia.MoveToward(Vector3.Zero, 0.5f * (float)delta);
		}


		return Entity.Gravity + Entity.Inertia;
	}

	protected abstract Vector3 ProcessGroundedMovement(double delta);


	protected virtual void HandleJump(double delta) {
		if (Entity.IsOnFloor()) {
			coyoteTimer.Start();
		}

		if (!jumpBuffer.HasElapsed && !coyoteTimer.HasElapsed) {
			ExecuteJump();
		}
	}

	protected void ExecuteJump() {
		if (JumpAction is null) return;

		if (jumpCooldown.HasElapsed && Entity.ExecuteAction(JumpAction) && Entity.CurrentAction is JumpAction jumpAction) {
			jumpAction.Direction = _jumpDirection;
			jumpBuffer.End();
			jumpCooldown.Start();
		}
	}
}