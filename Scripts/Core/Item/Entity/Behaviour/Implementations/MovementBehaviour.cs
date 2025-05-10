namespace LandlessSkies.Core;

using Godot;
using SevenDev.Boundless.Utility;


[Tool]
public abstract partial class MovementBehaviour : EntityBehaviour {
	protected override bool IsOneTime { get; } = false;

	protected MovementBehaviour() : this(null!) { }
	public MovementBehaviour(Entity entity) : base(entity) { }



	public abstract void Move(Vector3 movement, MovementType movementType = MovementType.Normal);

	protected void NormalizeRotation() {
		Vector3 upDirection = Entity.UpDirection;
		Vector3 globalForward = Entity.GlobalForward;
		globalForward = globalForward.SlideOnFace(upDirection).Normalized();
		Entity.GlobalForward = globalForward;

		Entity.GlobalBasis = Basis.LookingAt(globalForward, upDirection);
	}


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

		HandlePostMovement(delta);
	}

	protected virtual void HandleProcess(double delta) { }
	protected virtual Vector3 ProcessInertia(double delta) => Entity.Gravity + Entity.Inertia;
	protected virtual Vector3 ProcessMovement(double delta) => Entity.Movement;
	protected virtual void HandlePostMovement(double delta) { }



	public enum MovementType {
		Idle,
		Slow,
		Normal,
		Fast
	}
}