using Godot;
using System;
using System.Diagnostics.CodeAnalysis;


namespace LandlessSkies.Core;

[GlobalClass]
public abstract partial class EntityBehaviour : Node, IInputReader {
	[Export] public Entity Entity = null!;

	public abstract bool FreeOnStop { get; }



	public EntityBehaviour() : base() {}
	public EntityBehaviour([MaybeNull] Entity entity) : base() {
		ArgumentNullException.ThrowIfNull(entity);

		Entity = entity;
		Entity.AddChildAndSetOwner(this);
	}



	public virtual void HandleInput(Player.InputInfo inputInfo) {}

	public virtual bool SetSpeed(MovementSpeed speed) => true;
	public virtual bool Move(Vector3 direction) => true;
	public virtual bool Jump(Vector3? target = null) => true;

	public virtual void Start(EntityBehaviour? previousBehaviour) {}



	public enum MovementSpeed {
		Idle = 0,
		Walk = 1,
		Run = 2,
		Sprint = 3
	}
}