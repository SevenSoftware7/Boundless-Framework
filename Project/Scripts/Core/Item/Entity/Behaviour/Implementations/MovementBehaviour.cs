namespace LandlessSkies.Core;

using Godot;

[Tool]
public abstract partial class MovementBehaviour : EntityBehaviour {
	protected Vector3 _movement;
	protected override bool IsOneTime => false;

	protected MovementBehaviour() : this(null) { }
	public MovementBehaviour(Entity? entity) : base(entity) { }



	public void Move(Vector3 direction, MovementType movementType = MovementType.Run) {
		float oldMagnitude = _movement.LengthSquared();

		Vector3 newMovement = GetMovement(direction, movementType);
		float newMagnitude = newMovement.LengthSquared();

		_movement = (_movement + newMovement).Normalized() * Mathf.Sqrt(Mathf.Max(oldMagnitude, newMagnitude));
	}

	protected virtual Vector3 GetMovement(Vector3 direction, MovementType movementType) {
		float speed = movementType switch {
			MovementType.Walk => Entity.Stats.SlowSpeed,
			MovementType.Sprint => Entity.Stats.SprintSpeed,
			MovementType.Run or _ => Entity.Stats.BaseSpeed,
		};
		return direction * speed;
	}

	public override void _Process(double delta) {
		base._Process(delta);

		HandleProcess(delta);

		if (IsActive) {
			Entity.Inertia = ProcessInertia(delta);
			Entity.Movement = ProcessMovement(delta);

			Entity.Velocity = Entity.Inertia + Entity.Movement;
			Entity.MoveAndSlide();

			Entity.Movement = _movement = Vector3.Zero;
		}
	}

	protected virtual void HandleProcess(double delta) { }
	protected virtual Vector3 ProcessInertia(double delta) => Entity.Inertia;
	protected virtual Vector3 ProcessMovement(double delta) => Entity.Movement;



	public enum MovementType {
		Walk,
		Run,
		Sprint
	}
}