namespace LandlessSkies.Core;

using Godot;

[Tool]
public abstract partial class MovementBehaviour : EntityBehaviour {
	protected override bool IsOneTime { get; } = false;

	protected MovementBehaviour() : this(null!) { }
	public MovementBehaviour(Entity entity) : base(entity) { }

	protected override void _Start(EntityBehaviour? previousBehaviour = null) {
		base._Start(previousBehaviour);

		_ResetMovement(previousBehaviour);
	}
	protected abstract void _ResetMovement(EntityBehaviour? previousBehaviour = null);



	public abstract void Move(Vector3 movement, MovementType movementType = MovementType.Normal);


	public override void _Process(double delta) {
		base._Process(delta);

		HandleProcess(delta);

		if (IsActive) {
			Entity.Velocity = ProcessMovement(delta) + ProcessInertia(delta);
			Entity.MoveAndSlide();

			if (Entity.MotionMode == CharacterBody3D.MotionModeEnum.Grounded && Entity.Gravity.Dot(Entity.UpDirection) < 0) {
				Entity.ApplyFloorSnap();
			}

			Entity.Movement = Vector3.Zero;
		}
	}

	protected virtual void HandleProcess(double delta) { }
	protected virtual Vector3 ProcessInertia(double delta) => Entity.Gravity + Entity.Inertia;
	protected virtual Vector3 ProcessMovement(double delta) => Entity.Movement;



	public enum MovementType {
		Idle,
		Slow,
		Normal,
		Fast
	}
}