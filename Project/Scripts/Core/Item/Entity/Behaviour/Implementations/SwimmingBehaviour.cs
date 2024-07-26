namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;
using static Godot.CharacterBody3D;

[Tool]
public abstract partial class SwimmingBehaviour : MovementBehaviour, IPlayerHandler {
	private float _moveSpeed;
	protected Vector3 _moveDirection;


	protected SwimmingBehaviour() : base() { }
	public SwimmingBehaviour(Entity entity) : base(entity) { }


	protected override void _Start(EntityBehaviour? previousBehaviour) {
		Entity.GlobalForward = Entity.GlobalForward.SlideOnFace(Entity.UpDirection).Normalized();
		Entity.GlobalBasis = Basis.LookingAt(Entity.GlobalForward, Entity.UpDirection);

		Entity.MotionMode = MotionModeEnum.Floating;
	}
	protected override void _Stop() {
		DisavowPlayer();
	}


	public virtual void HandlePlayer(Player player) {
		Vector2 input = player.InputDevice.GetVector(
			Inputs.MoveLeft, Inputs.MoveRight,
			Inputs.MoveForward, Inputs.MoveBackward
		).ClampMagnitude(1f);
		player.CameraController.RawInputToCameraRelativeMovement(input, out _, out Vector3 movement);

		if (player.InputDevice.IsActionJustPressed(Inputs.Jump)) {
			movement += Entity.UpDirection;
		}


		Move(movement);
	}

	public virtual void DisavowPlayer() {
		_moveDirection = Vector3.Zero;
	}



	// public virtual bool SetMovementType(MovementType speed) => true;
	public override bool Move(Vector3 direction) {
		_moveDirection = direction;
		return true;
	}

	protected override void HandleMovement(double delta) {
		Entity.Inertia.MoveToward(Vector3.Zero, (float)delta * 1.75f);
		// HandleWalk(delta);

		float floatDelta = (float)delta;


		// ---- Speed Calculation ----

		// float newSpeed = _movementType switch {
		// 	MovementType.Walk => Entity.Stats.SlowSpeed,
		// 	MovementType.Run => Entity.Stats.BaseSpeed,
		// 	MovementType.Sprint => Entity.Stats.SprintSpeed,
		// 	_ => 0f
		// };
		float newSpeed = Entity.Stats.BaseSpeed;
		newSpeed = Entity.AttributeModifiers.ApplyTo(Attributes.GenericMoveSpeed, newSpeed);

		float speedDelta = _moveSpeed < newSpeed ? Entity.Stats.Acceleration : Entity.Stats.Deceleration;
		_moveSpeed = Mathf.MoveToward(_moveSpeed, newSpeed, speedDelta * floatDelta);


		// ----- Rotation & Movement -----
		float rotationSpeed = Entity.AttributeModifiers.ApplyTo(Attributes.GenericTurnSpeed, Entity.Stats.RotationSpeed);

		// if (_movementType != MovementType.Idle) {
		// 	Vector3 normalizedInput = _moveDirection.Normalized();

		// 	_lastDirection = _lastDirection.Lerp(normalizedInput, rotationSpeed * floatDelta);
		// 	Entity.GlobalForward = Entity.GlobalForward.SafeSlerp(normalizedInput, rotationSpeed * floatDelta);

		// 	Entity.Movement = _lastDirection * _moveSpeed * Mathf.Clamp(_lastDirection.Dot(Entity.GlobalForward), 0f, 1f);
		// }
		// else {
		// 	Entity.Movement = _lastDirection * _moveSpeed;
		// }

		Entity.Movement = _moveDirection * _moveSpeed;

		Basis newRotation = Basis.LookingAt(Entity.GlobalForward, Entity.UpDirection);
		Entity.GlobalBasis = Entity.GlobalBasis.SafeSlerp(newRotation, (float)delta * rotationSpeed);
	}
}