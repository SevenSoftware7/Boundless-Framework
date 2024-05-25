namespace LandlessSkies.Vanilla;

using Godot;
using LandlessSkies.Core;
using SevenDev.Utility;

[Tool]
[GlobalClass]
public partial class VehicleBehaviour : GroundedBehaviour, IPlayerHandler {
	private float _moveSpeed;
	private Vector3 _modelUp = Vector3.Up;



	public override void _Process(double delta) {
		base._Process(delta);

		if (Engine.IsEditorHint()) return;


		float floatDelta = (float) delta;


		float newSpeed = Entity.GlobalForward.Dot(_inputDirection) * Entity.AttributeModifiers.Get(Attributes.GenericMoveSpeed).ApplyTo(Entity.Stats.BaseSpeed);
		float speedDelta = newSpeed > _moveSpeed ? 1f : 0.25f;
		_moveSpeed = Mathf.MoveToward(_moveSpeed, newSpeed, speedDelta * Entity.Stats.Acceleration * floatDelta);

		if (_inputDirection.LengthSquared() != 0f)
			Entity.GlobalForward = Entity.GlobalForward.Slerp(_inputDirection.Normalized(), floatDelta * 3f).Normalized();

		Entity.Movement = Entity.GlobalForward * _moveSpeed;



		float groundFlatness = Entity.GetFloorNormal().Dot(Entity.UpDirection);

		Basis newRotation = Basis.LookingAt(Entity.GlobalForward, Entity.UpDirection);
		Entity.GlobalBasis = Entity.GlobalBasis.SafeSlerp(newRotation, (float)delta * Entity.Stats.RotationSpeed);

		if (Entity.Model is Model model) {
			Vector3 groundUp = groundFlatness > 0.5f ? Entity.GetFloorNormal() : Entity.UpDirection;
			Vector3 rightDir = Entity.GlobalForward.Cross(groundUp).Normalized();
			_modelUp = _modelUp.SafeSlerp((groundUp * 4f + _inputDirection.Dot(rightDir) * rightDir).Normalized(), 7f * floatDelta);
			Vector3 modelForward = _modelUp.Cross(rightDir);

			Basis modelRotation = Basis.LookingAt(modelForward, _modelUp);
			model.GlobalBasis = modelRotation;
		}
	}
}