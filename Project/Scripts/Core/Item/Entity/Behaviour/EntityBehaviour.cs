namespace LandlessSkies.Core;

using Godot;

[GlobalClass]
public abstract partial class EntityBehaviour : Node, IPlayerHandler {
	[Export] public Entity? Entity;


	protected EntityBehaviour() : base() { }
	public EntityBehaviour(Entity entity) : this() {
		Entity = entity;
	}


	public virtual bool Move(Vector3 direction) => true;

	public virtual void Start(EntityBehaviour? previousBehaviour) {
		ProcessMode = ProcessModeEnum.Inherit;
	}
	public virtual void Stop() {
		ProcessMode = ProcessModeEnum.Disabled;
		Callable.From(DisavowPlayer).CallDeferred();
	}

	public virtual void HandlePlayer(Player player) { }

	public virtual void DisavowPlayer() { }
}