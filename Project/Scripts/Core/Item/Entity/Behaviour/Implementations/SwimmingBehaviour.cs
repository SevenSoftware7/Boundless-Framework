namespace LandlessSkies.Core;

using System;
using System.Reflection.Metadata;
using Godot;
using SevenDev.Utility;
using static Godot.CharacterBody3D;

[Tool]
public partial class SwimmingBehaviour : MovementBehaviour, IPlayerHandler, IWaterCollisionNotifier, IWaterDisplacementSubscriber {
	private const float SurfaceThreshold = 2f;
	private const float CenterOfMassOffset = 2f;


	private float floatingDisplacement = 0f;

	private EntityBehaviour? previousBehaviour;
	private float _moveSpeed;
	protected Vector3 _moveDirection;

	[Export] private Water? Water;
	private float WaterSurface;
	private float WaterDisplacement;

	private bool _jumpInput;



	public float OffsetToWaterSurface => WaterSurface + WaterDisplacement - (Entity.CenterOfMass is null ? (Entity.GlobalPosition.Y + CenterOfMassOffset) : Entity.CenterOfMass.GlobalPosition.Y);
	public bool IsOnWaterSurface => Mathf.Abs(OffsetToWaterSurface) <= SurfaceThreshold;

	protected SwimmingBehaviour() : this(null!, null!) { }
	public SwimmingBehaviour(Entity entity, Water waterArea) : base(entity) {
		ArgumentNullException.ThrowIfNull(waterArea);
		Water = waterArea;
	}


	protected override void _Start(EntityBehaviour? previousBehaviour) {
		this.previousBehaviour = previousBehaviour;

		Entity.GlobalForward = Entity.GlobalForward.SlideOnFace(Entity.UpDirection).Normalized();
		Entity.GlobalBasis = Basis.LookingAt(Entity.GlobalForward, Entity.UpDirection);

		Entity.MotionMode = MotionModeEnum.Floating;

		WaterDisplacementEffect.Subscribers.Add(this);
	}
	protected override void _Stop(EntityBehaviour? nextBehaviour) {
		if (_jumpInput && OffsetToWaterSurface <= 0f && nextBehaviour is GroundedBehaviour groundedBehaviour) {
			groundedBehaviour.Jump(true);
		}

		DisavowPlayer();

		WaterDisplacementEffect.Subscribers.Remove(this);
	}


	public virtual void HandlePlayer(Player player) {
		if (!IsActive) return;

		Vector2 input = player.InputDevice.GetVector(
			Inputs.MoveLeft, Inputs.MoveRight,
			Inputs.MoveForward, Inputs.MoveBackward
		).ClampMagnitude(1f);
		player.CameraController.RawInputToCameraRelativeMovement(input, out _, out Vector3 movement);

		if (IsOnWaterSurface) {
			movement = movement.SlideOnFace(-Entity.UpDirection);
		}

		_jumpInput = player.InputDevice.IsActionPressed(Inputs.Jump);
		if (_jumpInput) {
			movement = (movement + Entity.UpDirection).ClampMagnitude(1f);
		}

		Move(movement);
	}

	public virtual void DisavowPlayer() {
		_moveDirection = Vector3.Zero;
		_jumpInput = false;
	}



	// public virtual bool SetMovementType(MovementType speed) => true;
	public override bool Move(Vector3 direction) {
		_moveDirection = direction;
		return true;
	}

	protected override void HandleMovement(double delta) {
		Entity.Inertia *= Mathf.Max(1 - 6f * (float)delta, 0f);

		float floatDelta = (float)delta;

		WaterSurface = Water?.GetSurfaceInDirection(Entity.GlobalPosition, Vector3.Up, out Collisions.IntersectRay3DResult result) ?? false
			? result.Point.Y
			: Mathf.Inf;

		float offsetToWaterSurface = OffsetToWaterSurface;
		float distanceToWaterSurface = Mathf.Abs(offsetToWaterSurface);
		bool isOnWaterSurface = distanceToWaterSurface <= SurfaceThreshold;

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


		// ----- Floating at the Surface -----
		if (isOnWaterSurface) {
			float floatingSpeed = Mathf.IsZeroApprox(Entity.Movement.LengthSquared())
				? (offsetToWaterSurface <= 0f ? 5f : 2.5f)
				: 1f;

			floatingDisplacement = Mathf.MoveToward(floatingDisplacement, offsetToWaterSurface, 2f * floatDelta);
			Vector3 floatingDisplacementVector = Vector3.Up * 2f * floatingDisplacement * floatingSpeed / (Mathf.Abs(distanceToWaterSurface) + 1f);

			Entity.Inertia = Entity.Inertia.MoveToward(floatingDisplacementVector, 12f * floatDelta);
		}
	}

	public void OnEnterWater(Water water) {
		Water ??= water;
	}

	public void OnExitWater(Water water) {
		if (!IsActive) return;
		if (water != Water) return;

		if (previousBehaviour is null) {
			Entity.SetBehaviour<GroundedBehaviour, BipedBehaviour>(() => new BipedBehaviour(Entity));
		}
		else {
			Entity.SetBehaviour(previousBehaviour);
		}
		Water = null;
	}

	public (Vector3 location, WaterMesh mesh)? GetInfo() => Water?.Mesh is null ? null : (Entity.GlobalPosition, Water.Mesh);
	public void UpdateWaterDisplacement(Vector3 waterDisplacement) => WaterDisplacement = waterDisplacement.Y;
}