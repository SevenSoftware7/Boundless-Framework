namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;

public abstract partial class GroundedBehaviour : EntityBehaviour, IPlayerHandler {
	protected Vector3 _inputDirection;

	protected readonly TimeDuration jumpBuffer = new(125);
	protected readonly TimeDuration coyoteTimer = new(150);
	protected readonly TimeDuration jumpCooldown = new(500);


	protected GroundedBehaviour() : base() { }
	public GroundedBehaviour(Entity entity) : base(entity) { }


	public override void Start(EntityBehaviour? previousBehaviour) {
		base.Start(previousBehaviour);
		if (Entity is null) return;

		Entity.GlobalForward = Entity.GlobalForward.SlideOnFace(Entity.UpDirection).Normalized();
		Entity.GlobalBasis = Basis.LookingAt(Entity.GlobalForward, Entity.UpDirection);

		Entity.MotionMode = CharacterBody3D.MotionModeEnum.Grounded;
	}


	public virtual void SetupPlayer(Player player) { }

	public virtual void HandlePlayer(Player player) {
		if (Entity is null) return;

		if (player.InputDevice.IsActionPressed(Inputs.Jump)) {
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
		if (! base.Move(direction)) return false;

		_inputDirection = direction;
		return true;
	}
	public virtual bool Jump(Vector3? target = null) {
		jumpBuffer.Start();
		return true;
	}



	public override void _Process(double delta) {
		base._Process(delta);

		if (Engine.IsEditorHint()) return;

		HandleInertia(delta);

		HandleJump(delta);
	}

	private void HandleInertia(double delta) {
		if (Entity is null) return;

		Entity.Inertia.Split(Entity.UpDirection, out Vector3 verticalInertia, out Vector3 horizontalInertia);

		horizontalInertia = ProcessHorizontalInertia(delta, horizontalInertia);
		verticalInertia = ProcessVerticalInertia(delta, verticalInertia);

		Entity.Inertia = verticalInertia + horizontalInertia;
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

		const float fallSpeed = 32f;

		float fallInertia = verticalInertia.Dot(-Entity.UpDirection);
		Vector3 targetInertia = -Entity.UpDirection * Mathf.Max(fallSpeed, fallInertia);

		if (verticalInertia.IsEqualApprox(targetInertia)) return verticalInertia;


		const float floatReductionFactor = 0.65f;
		const float fallIncreaseFactor = 1.75f;

		// Float more if player is holding jump key & rising
		float isFloating = jumpBuffer.IsDone
			? 0f
			: (1f - fallInertia).Clamp01();
		float floatFactor = Mathf.Lerp(1f, floatReductionFactor, isFloating);

		// Slightly ramp up inertia when falling
		float inertiaRampFactor = Mathf.Lerp(1f, fallIncreaseFactor, ((1f + fallInertia) * 0.5f).Clamp01());


		return verticalInertia.MoveToward(targetInertia, 45f * floatFactor * inertiaRampFactor * (float)delta);
	}

	protected virtual void HandleJump(double delta) {
		if (Entity is null) return;

		if (Entity.IsOnFloor()) {
			coyoteTimer.Start();
		}

		if (! jumpBuffer.IsDone && jumpCooldown.IsDone && !coyoteTimer.IsDone) {
			float jumpHeight = Entity.AttributeModifiers.Get(Attributes.GenericjumpHeight).ApplyTo(Entity.Stats.JumpHeight);

			Entity.Inertia = Entity.Inertia.SlideOnFace(Entity.UpDirection) + Entity.UpDirection * jumpHeight;
			jumpBuffer.End();
			jumpCooldown.Start();
		}
	}


	public enum MovementType {
		Idle = 0,
		Walk = 1,
		Run = 2,
		Sprint = 3
	}
}