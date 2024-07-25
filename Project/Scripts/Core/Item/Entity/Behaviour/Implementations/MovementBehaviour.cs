using Godot;

namespace LandlessSkies.Core;

public abstract partial class MovementBehaviour : EntityBehaviour {

	protected MovementBehaviour() : base() { }
	public MovementBehaviour(Entity entity, bool isOneTime = false) : base(entity, isOneTime) { }


	public virtual bool Move(Vector3 direction) => true;

	public override void _Process(double delta) {
		if (Entity is not Entity entity) return;

		base._Process(delta);

		HandleMovement(entity, delta);

		entity.Velocity = entity.Inertia + entity.Movement;

		entity.MoveAndSlide();
	}

	protected abstract void HandleMovement(Entity entity, double delta);
}