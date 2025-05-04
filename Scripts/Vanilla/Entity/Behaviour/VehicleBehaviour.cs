namespace LandlessSkies.Vanilla;

using System.Collections.Generic;
using Godot;
using LandlessSkies.Core;
using SevenDev.Boundless.Utility;
using static SevenDev.Boundless.Utility.Collisions;


[Tool]
[GlobalClass]
public partial class VehicleBehaviour : GroundedBehaviour, IWaterCollisionListener, IVoidOutListener {
	[Export] public DrivingBehaviour? Driver;

	private bool _gravityShift;
	private bool _drifting;

	private Vector3 _movement;
	private float _speed;
	private Vector3 _inertiaDirection;
	private float _inertia;

	private Vector3 _modelForward = Vector3.Forward;
	private Vector3 _modelUp = Vector3.Up;


	protected VehicleBehaviour() : this(null!) { }
	public VehicleBehaviour(Entity entity) : base(entity, new BipedJumpAction.Builder()) { }


	public override void Move(Vector3 movement, MovementType movementType = MovementType.Normal) {
		float speed = Entity.GetTraitValue(Traits.GenericMoveSpeed);
		_speed = movementType switch {
			MovementType.Slow => Entity.GetTraitValue(Traits.GenericSlowMoveSpeedMultiplier) * speed,
			MovementType.Fast => Entity.GetTraitValue(Traits.GenericFastMoveSpeedMultiplier) * speed,
			MovementType.Normal or _ => speed,
		};

		_movement = movement.Normalized();
	}



	protected override void _Start(EntityBehaviour? previousBehaviour) {
		base._Start(previousBehaviour);
	}
	protected override void _Stop(EntityBehaviour? nextBehaviour) {
		base._Stop(nextBehaviour);

		Driver?.Dismount();
	}

	public override void HandlePlayer(Player player) {
		base.HandlePlayer(player);
		_drifting = player.InputDevice.IsActionPressed(Inputs.AttackHeavy);
		if (player.InputDevice.IsActionJustPressed(Inputs.Focus)) {
			_gravityShift = !_gravityShift;
		}
	}

	protected override Vector3 ProcessGroundedMovement(double delta) {
		float floatDelta = (float)delta;

		float speedReverseLerp = Mathf.InverseLerp(0f, _speed, _inertia);
		bool isOnFloor = Entity.IsOnFloor();
		Vector3 entityUp = Entity.UpDirection;
		Vector3 entityForward = Entity.GlobalForward;

		Vector3 groundUp = entityUp;
		if (isOnFloor) {
			Vector3 entityPos = Entity.GlobalPosition;
			if (Entity.GetWorld3D().IntersectRay3D(entityPos + entityUp, entityPos - entityUp, out IntersectRay3DResult res, Entity.CollisionMask)) {
				groundUp = res.Normal.Normalized();
			}
		}


		if (!_gravityShift) {
			entityUp = entityUp.SafeSlerp(Vector3.Up, 18f * floatDelta);
		}
		else if (isOnFloor && groundUp.Dot(entityUp) >= 0.75f) {
			entityUp = entityUp.SafeSlerp(groundUp, 18f * floatDelta);
		}


		Basis upRotation = Entity.Transform.Up().FromToBasis(entityUp);

		entityForward = entityForward.SafeSlerp(upRotation * entityForward, 18f * floatDelta);
		_movement = upRotation * _movement;


		float newInertia = 0f;
		if (!_movement.IsEqualApprox(Vector3.Zero)) {
			newInertia = Mathf.Clamp(entityForward.Dot(_movement) + 0.75f, 0f, 1f) * _speed;
			entityForward = entityForward.Slerp(_movement, Entity.GetTraitValue(Traits.GenericTurnSpeed) * floatDelta);
		}


		if (_drifting) {
			newInertia = 0f;
		}
		else {
			_inertiaDirection = _inertiaDirection.Lerp(entityForward, 12f * floatDelta);
		}

		float speedDelta = newInertia > _inertia ? 1f : 0.25f;
		_inertia = _inertia.MoveToward(newInertia, speedDelta * Entity.GetTraitValue(Traits.GenericAcceleration) * floatDelta);



		if (Entity.CostumeHolder?.Costume is Costume model) {
			Basis toGroundUp = Entity.GlobalBasis.Up().FromToBasis(groundUp);
			Vector3 entityRight = toGroundUp * Entity.GlobalBasis.Right();

			Vector3 leanForward = toGroundUp * entityForward;
			if (!isOnFloor) leanForward = (leanForward + groundUp * 0.25f * speedReverseLerp).Normalized();

			Vector3 leanRight = _movement.Dot(entityRight) * entityRight;
			Vector3 leanUp = (entityUp + leanRight).Normalized();

			_modelForward = _modelForward.SafeSlerp(leanForward, 14f * floatDelta);
			_modelUp = _modelUp.SafeSlerp(leanUp, 10f * floatDelta);

			model.GlobalBasis = Basis.LookingAt(_modelForward, _modelUp);
		}


		Entity.UpDirection = entityUp;
		Entity.GlobalForward = entityForward;
		Entity.GlobalBasis = Basis.LookingAt(entityForward, Entity.UpDirection);

		_movement = Vector3.Zero;
		_speed = 0f;
		return _inertiaDirection * _inertia;
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
		_inertiaDirection = Vector3.Zero;
		_inertia = 0f;
	}
}