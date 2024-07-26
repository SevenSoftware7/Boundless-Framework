namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;

/// <summary>
/// A Hierarchical state machine node for an Entity's
/// </summary>
[Tool]
[GlobalClass]
public abstract partial class EntityBehaviour : Node {
	[Export] public Entity Entity;

	protected abstract bool IsOneTime { get; }


	protected EntityBehaviour() : this(null!) { }
	public EntityBehaviour(Entity entity) : base() {
		ProcessMode = ProcessModeEnum.Disabled;
		Entity = entity;
	}


	public void Start(EntityBehaviour? previousBehaviour = null) {
		if (Entity is null) {
			Stop();
			GD.PushError($"Could not start Behaviour {GetType().Name}, no reference to an Entity");
			return;
		}

		ProcessMode = ProcessModeEnum.Inherit;
		_Start(previousBehaviour is null || previousBehaviour.IsOneTime ? null : previousBehaviour);
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