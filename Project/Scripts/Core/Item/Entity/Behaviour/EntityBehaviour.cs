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

	public void Start(EntityBehaviour? previousBehaviour) {
		ProcessMode = ProcessModeEnum.Inherit;
		_Start(previousBehaviour);
	}
	public void Stop() {
		_Stop();
		ProcessMode = ProcessModeEnum.Disabled;
		Callable.From(DisavowPlayer).CallDeferred();
	}

	protected abstract void _Start(EntityBehaviour? previousBehaviour);
	protected abstract void _Stop();


	public virtual void HandlePlayer(Player player) { }
	public virtual void DisavowPlayer() { }
}