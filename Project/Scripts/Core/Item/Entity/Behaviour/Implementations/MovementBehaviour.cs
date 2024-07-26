using Godot;

namespace LandlessSkies.Core;

[Tool]
public abstract partial class MovementBehaviour : EntityBehaviour {

	protected override bool IsOneTime => false;

	protected MovementBehaviour() : base() { }
	public MovementBehaviour(Entity entity) : base(entity) { }


	public abstract bool Move(Vector3 direction);

	public sealed override void _Process(double delta) {
		if (Engine.IsEditorHint()) return;

		base._Process(delta);

		HandleMovement(delta);

		Entity.Velocity = Entity.Inertia + Entity.Movement;

		Entity.MoveAndSlide();
	}

	protected abstract void HandleMovement(double delta);
}