namespace LandlessSkies.Vanilla;

using Godot;
using LandlessSkies.Core;
using SevenDev.Boundless.Utility;

[Tool]
[GlobalClass]
public partial class VehicleBehaviour : GroundedBehaviour, IWaterCollisionNotifier {
	[Export] public DrivingBehaviour? Driver;

	private bool _drifting;

	private Vector3 _inertiaMovement;
	private float _moveSpeed;
	private Vector3 _modelUp = Vector3.Up;


	protected VehicleBehaviour() : this(null!) { }
	public VehicleBehaviour(Entity entity) : base(entity, new BipedJumpAction.Builder()) { }



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
	}

	protected override Vector3 ProcessGroundedMovement(double delta) {
		float floatDelta = (float)delta;


		float newSpeed = 0f;
		Vector3 direction = Vector3.Zero;
		if (!_movement.IsEqualApprox(Vector3.Zero)) {
			direction = _movement.Normalized();

			newSpeed = Mathf.Clamp(Entity.GlobalForward.Dot(direction) + 0.25f, 0f, 1f) * Entity.GetTraitValue(Traits.GenericMoveSpeed);
			Entity.GlobalForward = Entity.GlobalForward.Slerp(direction, Entity.GetTraitValue(Traits.GenericTurnSpeed) * floatDelta);
		}

		if (_drifting) {
			newSpeed = 0f;
		}
		else {
			_inertiaMovement = _inertiaMovement.Lerp(Entity.GlobalForward, 12f * floatDelta);
		}

		float speedDelta = newSpeed > _moveSpeed ? 1f : 0.25f;
		_moveSpeed = _moveSpeed.MoveToward(newSpeed, speedDelta * Entity.GetTraitValue(Traits.GenericAcceleration) * floatDelta);

		ComputeRotation(direction, floatDelta);

		_movement = Vector3.Zero;
		return Vector3.Zero;

		void ComputeRotation(Vector3 direction, float floatDelta) {
			Vector3 normal = Entity.GetFloorNormal();
			float groundFlatness = normal.Dot(Entity.UpDirection);

			if (Entity.CostumeHolder?.Costume is Costume model) {
				Vector3 groundUp = groundFlatness > 0.5f ? normal : Entity.UpDirection;
				Vector3 rightDir = Entity.GlobalForward.Cross(groundUp).Normalized();

				_modelUp = _modelUp.Lerp((groundUp + direction.Dot(rightDir) * rightDir).Normalized(), 7f * floatDelta).Normalized();

				Basis modelRotation = Basis.LookingAt(Entity.GlobalForward, _modelUp);
				model.GlobalBasis = modelRotation;
			}

			Basis newRotation = Basis.LookingAt(Entity.GlobalForward, Entity.UpDirection);
			Entity.GlobalBasis = newRotation;
		}
	}

	protected override Vector3 ProcessHorizontalInertia(double delta, Vector3 horizontalInertia) {
		return _inertiaMovement * _moveSpeed;
	}

	public void OnEnterWater(Water water) {
		Driver?.Dismount();
		Entity.VoidOut();
	}

	public void OnExitWater(Water water) { }
}