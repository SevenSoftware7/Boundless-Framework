namespace EndlessTwilight;

using Godot;
using SevenDev.Boundless;
using SevenDev.Boundless.Utility;
using static SevenDev.Boundless.Utility.Collisions;


[Tool]
[GlobalClass]
public partial class VehicleBehaviour : GroundedBehaviour, IWaterCollisionListener, IVoidOutListener {
	[Export] public DrivingBehaviour? Driver;

	private bool _isGravityShifting;
	private bool _isDrifting;

	private Vector3 _targetDirection;
	private float _targetSpeed;
	private Vector3 _currentDirection;
	private float _currentSpeed;

	private Vector3 _modelForward = Vector3.Forward;
	private Vector3 _modelUp = Vector3.Up;
	private float _modelLean = 0f;


	protected VehicleBehaviour() : this(null!) { }
	public VehicleBehaviour(Entity entity) : base(entity, new BipedJumpAction.Builder()) { }


	public override void Move(Vector3 movement, MovementType movementType = MovementType.Normal) {
		float speed = Entity.GetTraitValue(Traits.GenericMoveSpeed);
		_targetSpeed = movementType switch {
			MovementType.Slow => Entity.GetTraitValue(Traits.GenericSlowMoveSpeedMultiplier) * speed,
			MovementType.Fast => Entity.GetTraitValue(Traits.GenericFastMoveSpeedMultiplier) * speed,
			MovementType.Normal or _ => speed,
		};

		_targetDirection = movement.Normalized();
	}


	protected override void _ResetMovement(EntityBehaviour? previousBehaviour = null) {
		_currentSpeed = Entity.Movement.Length();
	}
	protected override void _Stop(EntityBehaviour? nextBehaviour) {
		base._Stop(nextBehaviour);

		Driver?.Dismount();
	}

	public override void HandlePlayer(Player player) {
		base.HandlePlayer(player);
		_isDrifting = player.InputDevice.IsActionPressed(Inputs.AttackHeavy);
		if (player.InputDevice.IsActionJustPressed(Inputs.Focus)) {
			_isGravityShifting = !_isGravityShifting;
		}
	}

	protected override Vector3 ProcessUpDirection(double delta) {
		bool isOnFloor = Entity.IsOnFloor();
		Vector3 entityUp = Entity.UpDirection;

		Vector3 groundUp = entityUp;
		if (isOnFloor) {
			Vector3 entityPos = Entity.GlobalPosition;
			if (Entity.GetWorld3D().IntersectRay3D(entityPos + entityUp, entityPos - entityUp, out IntersectRay3DResult res, Entity.CollisionMask)) {
				groundUp = res.Normal.Normalized();
			}
		}


		if (!_isGravityShifting) {
			entityUp = Vector3.Up;
		}
		else if (isOnFloor && groundUp.Dot(entityUp) >= 0.75f) {
			entityUp = groundUp;
		}
		return entityUp;
	}

	protected override Vector3 ProcessGroundedMovement(double delta) {
		float floatDelta = (float)delta;

		float speedReverseLerp = Mathf.InverseLerp(0f, _targetSpeed, _currentSpeed);
		Basis rotation = Entity.GlobalBasis;
		Vector3 forward = rotation.Forward();

		float rotationSpeed = Entity.GetTraitValue(Traits.GenericTurnSpeed);

		float newSpeed = 0f;
		if (!_targetDirection.IsZeroApprox()) {
			newSpeed = Mathf.Clamp(forward.Dot(_targetDirection) + 0.75f, 0f, 1f) * _targetSpeed;
			forward = forward.Slerp(_targetDirection, rotationSpeed * floatDelta);
		}

		Entity.GlobalBasis = rotation = BasisExtensions.UpTowards(rotation.Up(), forward);
		forward = rotation.Forward();



		if (_isDrifting) {
			newSpeed = 0f;
		}
		else {
			_currentDirection = _currentDirection.Lerp(forward, 12f * floatDelta);
		}

		float speedDelta = newSpeed > _currentSpeed ? Entity.GetTraitValue(Traits.GenericAcceleration) : Entity.GetTraitValue(Traits.GenericDeceleration);
		_currentSpeed = _currentSpeed.MoveToward(newSpeed, speedDelta * floatDelta);



		if (Entity.CostumeHolder?.Costume is Costume model) {
			Vector3 up = rotation.Up();
			Vector3 right = rotation.Right();
			Vector3 leanForward = forward;

			if (!Entity.IsOnFloor()) leanForward = (leanForward + up * 0.25f * speedReverseLerp).Normalized();

			_modelForward = _modelForward.SafeSlerp(leanForward, 18f * floatDelta);
			_modelUp = _modelUp.SafeSlerp(up, 18f * floatDelta);

			_modelLean = _modelLean.Lerp(_targetDirection.Dot(right), 4f * floatDelta);
			Vector3 finalUp = (up + _modelLean * right).Normalized();

			model.GlobalBasis = Basis.LookingAt(_modelForward, finalUp);
		}

		_targetDirection = Vector3.Zero;
		_targetSpeed = 0f;
		return _currentDirection * _currentSpeed;
	}

	// protected override Vector3 ProcessInertia(double delta, Vector3 horizontalInertia) {
	// 	return _inertiaMovement * _moveSpeed;
	// }

	public void OnEnterWater(Water water) {
		Driver?.Dismount();
		Entity.VoidOut();
	}

	public void OnExitWater(Water water) { }

	public void OnVoidOut(Entity entity) {
		if (entity != Entity) return;
		_currentDirection = Vector3.Zero;
		_currentSpeed = 0f;
	}
}