namespace LandlessSkies.Core;

using System;
using Godot;

/// <summary>
/// A Hierarchical state machine node for an Entity's Behaviour
/// </summary>
[Tool]
[GlobalClass]
public abstract partial class EntityBehaviour : Behaviour<EntityBehaviour> {
	[Export] public Entity Entity;



	protected EntityBehaviour() : this(null) { }
	public EntityBehaviour(Entity? entity) : base() {
		Entity = entity!;
	}


	protected override void _Start(EntityBehaviour? previousBehaviour = null) {
		if (Entity is null) {
			Stop();
			throw new ArgumentNullException($"Could not start Behaviour {GetType().Name}, no reference to an Entity");
		}

		base._Start(previousBehaviour);
	}
}