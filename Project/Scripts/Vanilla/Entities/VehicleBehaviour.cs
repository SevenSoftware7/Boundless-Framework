namespace LandlessSkies.Vanilla;

using Godot;
using LandlessSkies.Core;
using SevenDev.Utility;

[Tool]
[GlobalClass]
public partial class VehicleBehaviour : GroundedBehaviour, IPlayerHandler {
	private float _moveSpeed;
	private Vector3 _modelUp = Vector3.Up;


	protected VehicleBehaviour() : base() { }
	public VehicleBehaviour(Entity entity) : base(entity) { }


	public override void _Process(double delta) {
		base._Process(delta);

		if (Engine.IsEditorHint()) return;
		if (Entity is null) return;

		float floatDelta = (float) delta;

		float newSpeed = 0f;
		if (! _inputDirection.IsEqualApprox(Vector3.Zero)) {
			Vector3 direction = _inputDirection.Normalized();

			newSpeed = Entity.GlobalForward.Dot(direction) * Entity.AttributeModifiers.Get(Attributes.GenericMoveSpeed).ApplyTo(Entity.Stats.BaseSpeed);
			Entity.GlobalForward = Entity.GlobalForward.Slerp(direction, floatDelta * 3f);
		}

		float speedDelta = newSpeed > _moveSpeed ? 1f : 0.25f;
		_moveSpeed = Mathf.MoveToward(_moveSpeed, newSpeed, speedDelta * Entity.Stats.Acceleration * floatDelta);

		Entity.Movement = Entity.GlobalForward * _moveSpeed;

		Vector3 normal = Entity.GetFloorNormal();
		float groundFlatness = normal.Dot(Entity.UpDirection);

		if (Entity.Model is Model model) {
			Vector3 groundUp = groundFlatness > 0.5f ? normal : Entity.UpDirection;
			Vector3 rightDir = Entity.GlobalForward.Cross(groundUp).Normalized();

			_modelUp = _modelUp.SafeSlerp((groundUp * 4f + _inputDirection.Dot(rightDir) * rightDir).Normalized(), 7f * floatDelta);
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
	}
}