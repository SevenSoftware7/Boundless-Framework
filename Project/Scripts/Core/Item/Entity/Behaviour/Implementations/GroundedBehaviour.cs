namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;

public abstract partial class GroundedBehaviour : EntityBehaviour {
	protected JumpActionInfo? JumpAction { get; init; }
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


	public override void HandlePlayer(Player player) {
		base.HandlePlayer(player);
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

	public override void DisavowPlayer() {
		base.DisavowPlayer();
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

		const float fallIncreaseFactor = 1.75f;

		// Slightly ramp up inertia when falling
		float inertiaRampFactor = Mathf.Lerp(1f, fallIncreaseFactor, ((1f + fallInertia) * 0.5f).Clamp01());


		return verticalInertia.MoveToward(targetInertia, 45f * inertiaRampFactor * (float)delta);
	}

	protected virtual void HandleJump(double delta) {
		if (Entity is null) return;

		if (Entity.IsOnFloor()) {
			coyoteTimer.Start();
		}

		if (! jumpBuffer.IsDone && jumpCooldown.IsDone && !coyoteTimer.IsDone) {
			if (JumpAction is not null) {
				Entity.ExecuteAction(new JumpActionBuilder(JumpAction, Entity.UpDirection));
			}
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