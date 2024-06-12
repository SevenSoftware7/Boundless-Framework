namespace LandlessSkies.Core;

using Godot;

public abstract partial class EntityTrigger : Area3D {
	[Signal] public delegate void EntityEnteredEventHandler(Entity entity);


	private void OnBodyEntered(Node3D body) {
		if (body is Entity entity) {
			OnEntityEntered(entity);
			EmitSignal(SignalName.EntityEntered, entity);
		}
	}

	protected abstract void OnEntityEntered(Entity entity);


	public override void _Ready() {
		base._Ready();
		BodyEntered += OnBodyEntered;
	}
}