namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;

/// <summary>
/// A Hierarchical state machine node, meant for use with BehaviourTree
/// </summary>
[Tool]
public abstract partial class Behaviour<T> : Node where T : Behaviour<T> {

	public bool IsActive => _isActive && !Engine.IsEditorHint();
	private bool _isActive = false;

	protected abstract bool IsOneTime { get; }


	public Behaviour() : base() {
		_isActive = false;
	}


	public void Start(T? previousBehaviour = null) {
		// GD.Print($"Start {GetType().Name}");

		_Start(previousBehaviour is null || previousBehaviour.IsOneTime ? null : previousBehaviour);
		_isActive = true;
	}
	public void Stop(T? NextBehaviour = null) {
		// GD.Print($"Stop {GetType().Name}");

		_Stop(NextBehaviour);
		_isActive = false;

		if (IsOneTime) {
			this.UnparentAndQueueFree();
		}
		else {
			Name = $"{GetType().Name}_inactive";
		}
	}

	protected virtual void _Start(T? previousBehaviour) { }
	protected virtual void _Stop(T? nextBehaviour) { }
}