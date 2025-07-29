namespace Seven.Boundless;

using Godot;
using Seven.Boundless.Utility;

[Tool]
[GlobalClass]
public partial class SwimmingBehaviour : MovementBehaviour, IPlayerHandler, IWaterCollisionListener, IWaterDisplacementSubscriber {
	private const float SurfaceThreshold = 2f;
	private const float CenterOfMassOffset = 2f;



	[Export] private Water? Water;
	private EntityBehaviour? previousBehaviour;

	private Vector3 _targetMovement;
	private Vector3 _currentMovement;
	private float _targetSpeed;
	private float _currentSpeed;

	private float _floatingDisplacement = 0f;

	private float _waterSurface;
	private float _waterDisplacement;

	private bool _jumpInput;


	protected sealed override CharacterBody3D.MotionModeEnum MotionMode => CharacterBody3D.MotionModeEnum.Floating;

	public float GetEntityCenterOfMassHeight() => Entity.CenterOfMass is null ? (Entity.GlobalPosition.Y + CenterOfMassOffset) : Entity.CenterOfMass.GlobalPosition.Y;
	public float GetWaterSurfaceHeight() => _waterSurface + _waterDisplacement;
	public float GetOffsetToWaterSurface() => GetWaterSurfaceHeight() - GetEntityCenterOfMassHeight();
	public bool GetIsOnWaterSurface() => Mathf.Abs(GetOffsetToWaterSurface()) <= SurfaceThreshold;

	protected SwimmingBehaviour() : this(null!, null!) { }
	public SwimmingBehaviour(Entity entity, Water waterArea) : base(entity) {
		Water = waterArea;
	}


	protected override void _ResetMovement(EntityBehaviour? previousBehaviour = null) {
		Vector3 movement = Entity.Movement + Entity.Inertia + Entity.Gravity;
		_currentSpeed = movement.Length();
		_currentMovement = movement / _currentSpeed;
	}

	public (Vector3 location, WaterMesh mesh)? GetInfo() => Water?.Mesh is null ? null : (Entity.GlobalPosition, Water.Mesh);
	public void UpdateWaterDisplacement(Vector3 waterDisplacement) => _waterDisplacement = waterDisplacement.Y;

	private void UpdateWaterSurfaceHeight() {
		_waterSurface = Water?.GetSurfaceInDirection(Entity.GlobalPosition, Vector3.Up, out Collisions.IntersectRay3DResult result) ?? false
			? result.Point.Y
			: Mathf.Inf;
	}


	public override void Move(Vector3 movement, MovementType movementType = MovementType.Normal) {
		float speed = Entity.GetTraitValue(Traits.GenericMoveSpeed);
		_targetSpeed = movementType switch {
			MovementType.Slow => Entity.GetTraitValue(Traits.GenericSlowMoveSpeedMultiplier) * speed,
			MovementType.Fast => Entity.GetTraitValue(Traits.GenericFastMoveSpeedMultiplier) * speed,
			MovementType.Normal or _ => speed,
		};
		_targetMovement = movement.Normalized();
	}


	protected override Vector3 ProcessInertia(double delta) {
		float mult = Mathf.Max(1 - 6f * (float)delta, 0f);
		return (Entity.Gravity *= mult) + (Entity.Inertia *= mult);
	}

	protected override Vector3 ProcessMovement(double delta) {
		float floatDelta = (float)delta;
		UpdateWaterSurfaceHeight();

		float surfaceHeight = GetWaterSurfaceHeight();
		float offsetToWaterSurface = surfaceHeight - GetEntityCenterOfMassHeight();
		float distanceToWaterSurface = Mathf.Abs(offsetToWaterSurface);
		bool isOnWaterSurface = distanceToWaterSurface <= SurfaceThreshold;


		_currentMovement = _targetMovement;

		float speedDelta = Entity.GetTraitValue(_currentSpeed < _targetSpeed ? Traits.GenericAcceleration : Traits.GenericDeceleration);
		_currentSpeed = _currentSpeed.MoveToward(_targetSpeed, speedDelta * floatDelta);


		// ----- Rotation & Movement -----
		float rotationSpeed = Entity.GetTraitValue(Traits.GenericTurnSpeed);

		Basis rotation = Entity.GlobalBasis;
		Entity.GlobalBasis = rotation = rotation.SafeSlerp(Basis.LookingAt(rotation.Forward(), Entity.UpDirection), floatDelta * rotationSpeed);


		// ----- Floating at the Surface -----
		if (isOnWaterSurface) {
			float floatingSpeed = Mathf.IsZeroApprox(Entity.Movement.LengthSquared())
				? (offsetToWaterSurface <= 0f ? 5f : 2.5f)
				: 1f;

			_floatingDisplacement = _floatingDisplacement.MoveToward(offsetToWaterSurface, 2f * floatDelta);
			Vector3 floatingDisplacementVector = Vector3.Up * 2f * _floatingDisplacement * floatingSpeed / (Mathf.Abs(distanceToWaterSurface) + 1f);

			Entity.Gravity = Entity.Gravity.MoveToward(floatingDisplacementVector, 12f * floatDelta);
		}

		_targetMovement = Vector3.Zero;
		_targetSpeed = 0f;
		return Entity.Movement = _currentMovement * _currentSpeed;
	}


	private void ExitWater() {
		if (previousBehaviour is null) {
			Entity.SetBehaviourOrReset<GroundedBehaviour>();
		}
		else {
			Entity.SetBehaviour(previousBehaviour);
		}
	}

	public override void _Process(double delta) {
		if (!Engine.IsEditorHint() && Water is not null) {
			bool isSubmerged = GetEntityCenterOfMassHeight() < _waterSurface;
			bool hasFoothold = Entity.IsOnWall() && Entity.GetWallNormal().Dot(Entity.UpDirection) >= 0f;
			if (IsActive && !isSubmerged && hasFoothold) {
				ExitWater();
			}
			else if (!IsActive && isSubmerged) {
				Entity.SetBehaviour(this);
			}
		}
		base._Process(delta);
	}

	protected override void _Start(EntityBehaviour? previousBehaviour) {
		base._Start(previousBehaviour);

		this.previousBehaviour = previousBehaviour;

		WaterDisplacementEffect.Subscribers.Add(this);
	}
	protected override void _Stop(EntityBehaviour? nextBehaviour) {
		base._Stop(nextBehaviour);

		if (_jumpInput && GetOffsetToWaterSurface() <= 0f && nextBehaviour is GroundedBehaviour groundedBehaviour) {
			groundedBehaviour.Jump(force: true);
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
		_jumpInput = player.InputDevice.IsActionPressed(Inputs.Jump);


		bool hasMovementInput = player.CameraController.GetCameraRelativeMovement(input, out _, out Vector3 inputMovement);
		if (hasMovementInput && GetIsOnWaterSurface()) {
			inputMovement = inputMovement.SlideOnFace(-Entity.UpDirection);
		}

		if (_jumpInput) {
			inputMovement = (inputMovement + Entity.UpDirection).ClampMagnitude(1f);
		}

		if (hasMovementInput || _jumpInput) {
			Move(inputMovement);
		}


		if (player.CameraController.SetOrAddBehaviour<GravitatedCameraBehaviour>(
			() => new(player.CameraController),
			out var cameraBehaviour)
		) {
			cameraBehaviour.Subject = Entity;
			cameraBehaviour.MoveCamera(
				player.InputDevice.GetVector(Inputs.LookLeft, Inputs.LookRight, Inputs.LookDown, Inputs.LookUp) * player.InputDevice.Sensitivity
			);
		}
	}

	public virtual void DisavowPlayer() {
		_jumpInput = false;
	}


	public void OnEnterWater(Water water) {
		Water ??= water;
		UpdateWaterSurfaceHeight();
	}

	public void OnExitWater(Water water) {
		if (water != Water) return;

		Water = null;
		ExitWater();
	}
}