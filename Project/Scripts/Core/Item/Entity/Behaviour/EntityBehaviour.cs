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

	public bool IsActive => _isActive && !Engine.IsEditorHint();
	private bool _isActive = false;

	protected abstract bool IsOneTime { get; }


	protected EntityBehaviour() : this(null!) { }
	public EntityBehaviour(Entity entity) : base() {
		_isActive = false;

		Entity = entity;
	}


	public void Start(EntityBehaviour? previousBehaviour = null) {
		if (Entity is null) {
			Stop();
			GD.PushError($"Could not start Behaviour {GetType().Name}, no reference to an Entity");
			return;
		}

		_isActive = true;

		_Start(previousBehaviour is null || previousBehaviour.IsOneTime ? null : previousBehaviour);
	}
	public void Stop(EntityBehaviour? NextBehaviour = null) {
		_isActive = false;

		_Stop(NextBehaviour);

		if (IsOneTime) {
			this.UnparentAndQueueFree();
		}
	}

	protected abstract void _Start(EntityBehaviour? previousBehaviour);
	protected abstract void _Stop(EntityBehaviour? nextBehaviour);
}