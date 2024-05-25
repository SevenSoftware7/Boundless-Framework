namespace LandlessSkies.Core;

using System;
using Godot;
using SevenDev.Utility;

[GlobalClass]
public abstract partial class EntityBehaviour : Node {
	[Export] public Entity Entity;


	protected EntityBehaviour() : this(null!) { }
	public EntityBehaviour(Entity entity) {
		Entity = entity;
	}


	public virtual bool SetMovementType(MovementType speed) => true;
	public virtual bool Move(Vector3 direction) => true;
	public virtual bool Jump(Vector3? target = null) => true;

	public virtual void Start(EntityBehaviour? previousBehaviour) {
		ProcessMode = ProcessModeEnum.Inherit;
	}
	public virtual void Stop() {
		ProcessMode = ProcessModeEnum.Disabled;
	}



	public enum MovementType {
		Idle = 0,
		Walk = 1,
		Run = 2,
		Sprint = 3
	}
}