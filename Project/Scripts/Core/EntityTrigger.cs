namespace LandlessSkies.Core;

using System;
using Godot;

public abstract partial class EntityTrigger : Area3D {
	[Signal] public delegate void EntityEnteredEventHandler(Entity entity);

	public override void _Ready() {
		base._Ready();
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node3D body) {
		if (body is Entity entity) {
			OnEntityEntered(entity);
			EmitSignal(SignalName.EntityEntered, entity);
		}
	}

	protected abstract void OnEntityEntered(Entity entity);
}