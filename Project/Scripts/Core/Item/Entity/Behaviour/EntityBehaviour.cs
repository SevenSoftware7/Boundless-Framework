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


	public virtual bool Move(Vector3 direction) => true;

	public virtual void Start(EntityBehaviour? previousBehaviour) {
		ProcessMode = ProcessModeEnum.Inherit;
	}
	public virtual void Stop() {
		ProcessMode = ProcessModeEnum.Disabled;
	}
}