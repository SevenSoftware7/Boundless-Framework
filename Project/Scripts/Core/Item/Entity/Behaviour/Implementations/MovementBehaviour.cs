namespace LandlessSkies.Core;

using Godot;

[Tool]
public abstract partial class MovementBehaviour : EntityBehaviour {

	protected override bool IsOneTime => false;

	protected MovementBehaviour() : base() { }
	public MovementBehaviour(Entity entity) : base(entity) { }


	public abstract bool Move(Vector3 direction);

	public sealed override void _Process(double delta) {
		base._Process(delta);

		HandleProcess(delta);

		if (IsActive) {
			HandleMovement(delta);

			Entity.Velocity = Entity.Inertia + Entity.Movement;

			Entity.MoveAndSlide();
		}
	}

	protected virtual void HandleProcess(double delta) { }
	protected abstract void HandleMovement(double delta);
}