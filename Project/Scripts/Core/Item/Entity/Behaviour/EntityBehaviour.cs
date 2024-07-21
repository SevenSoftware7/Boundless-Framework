namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;

/// <summary>
/// A Hierarchical state machine node for an Entity's
/// </summary>
[GlobalClass]
public abstract partial class EntityBehaviour : Node {
	[Export] public Entity? Entity;

	public bool IsOneTime { get; }


	protected EntityBehaviour() : base() { }
	public EntityBehaviour(Entity entity, bool isOneTime = false) : this() {
		Entity = entity;
		IsOneTime = isOneTime;
	}


	public virtual bool Move(Vector3 direction) => true;

	public void Start(EntityBehaviour? previousBehaviour) {
		ProcessMode = ProcessModeEnum.Inherit;
		_Start(previousBehaviour);
	}
	public void Stop() {
		ProcessMode = ProcessModeEnum.Disabled;
		_Stop();
		if (IsOneTime) {
			this.UnparentAndQueueFree();
		}
	}

	protected abstract void _Start(EntityBehaviour? previousBehaviour);
	protected abstract void _Stop();
}