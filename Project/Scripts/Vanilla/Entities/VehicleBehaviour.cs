namespace LandlessSkies.Vanilla;

using Godot;
using LandlessSkies.Core;
using SevenDev.Utility;

[Tool]
[GlobalClass]
public partial class VehicleBehaviour : GroundedBehaviour, IWaterCollisionNotifier {
	[Export] public DrivingBehaviour? Driver;

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

	protected override Vector3 ProcessGroundedMovement(double delta) {
		float floatDelta = (float)delta;


		float newSpeed = 0f;
		Vector3 direction = Vector3.Zero;
		if (!_movement.IsEqualApprox(Vector3.Zero)) {
			direction = _movement.Normalized();

			newSpeed = Entity.GlobalForward.Dot(direction) * Entity.AttributeModifiers.ApplyTo(Attributes.GenericMoveSpeed, Entity.Stats.BaseSpeed);
			Entity.GlobalForward = Entity.GlobalForward.Slerp(direction, floatDelta * 3f);
		}

		float speedDelta = newSpeed > _moveSpeed ? 1f : 0.25f;
		_moveSpeed = _moveSpeed.MoveToward(newSpeed, speedDelta * Entity.Stats.Acceleration * floatDelta);


		Vector3 normal = Entity.GetFloorNormal();
		float groundFlatness = normal.Dot(Entity.UpDirection);

		if (Entity.CostumeHolder?.Model is Model model) {
			Vector3 groundUp = groundFlatness > 0.5f ? normal : Entity.UpDirection;
			Vector3 rightDir = Entity.GlobalForward.Cross(groundUp).Normalized();

			_modelUp = _modelUp.SafeSlerp((groundUp * 4f + direction.Dot(rightDir) * rightDir).Normalized(), 7f * floatDelta);
			Vector3 modelForward = _modelUp.Cross(rightDir);

			Basis modelRotation = Basis.LookingAt(modelForward, _modelUp);
			model.GlobalBasis = modelRotation;

			Basis realRotation = Basis.LookingAt(Entity.GlobalForward, Entity.UpDirection);
			Entity.GlobalBasis = realRotation;
		}
		else {
			Basis newRotation = Basis.LookingAt(Entity.GlobalForward, Entity.UpDirection);
			Entity.GlobalBasis = Entity.GlobalBasis.SafeSlerp(newRotation, (float)delta * Entity.Stats.RotationSpeed);
		}

		_movement = Vector3.Zero;
		return Entity.GlobalForward * _moveSpeed;
	}

	public void OnEnterWater(Water water) {
		Driver?.Dismount();
		Entity.VoidOut();
	}

	public void OnExitWater(Water water) { }
}