namespace Seven.Boundless;

using Godot;
using Seven.Boundless.Utility;

/// <summary>
/// A Hierarchical state machine node, meant for use as a Node
/// </summary>
[Tool]
public abstract partial class Behaviour<T> : Node where T : Behaviour<T> {

	/// <summary>
	/// Returns whether the Behaviour is active/running as the 'current' behaviour.<para/>
	/// This will always return false in Editor mode.<para/>
	/// Processing can still run when the behaviour is inactive, so you need to check if the behaviour is active before executing it fully.
	/// </summary>
	public bool IsActive {
		get => field && !Engine.IsEditorHint();
		private set;
	} = false;


	/// <summary>
	/// Returns whether the behaviour is a 'one-time' behaviour and will be destroyed upon being stopped.<para/>
	/// </summary>
	protected abstract bool IsOneTime { get; }

	/// <summary>
	/// </summary>
	protected Behaviour() : base() { }


	/// <summary>
	/// Start the Behaviour's activity.<para/>
	/// This will change the state of <see cref="IsActive"></see> to `true` and invoke the callback method <see cref="_Start"></see>.
	/// </summary>
	/// <param name="previousBehaviour">The last behaviour to be active before this one.</param>
	public void Start(T? previousBehaviour = null) {
		if (IsActive) return;
		IsActive = true;

		_Start((previousBehaviour is null || previousBehaviour.IsOneTime) ? null : previousBehaviour);
	}
	/// <summary>
	/// Stop the Behaviour's activity.<para/>
	/// This will change the state of <see cref="IsActive"></see> to `false` and invoke the callback method <see cref="_Stop"></see>.<para/>
	/// If this behaviour is a one-time behaviour (see <see cref="IsOneTime"></see>), this will free the behaviour.
	/// </summary>
	/// <param name="nextBehaviour">The next behaviour to be activated after this one.</param>
	public void Stop(T? nextBehaviour = null) {
		if (!IsActive) return;
		IsActive = false;

		_Stop(nextBehaviour);

		if (IsOneTime) {
			this.UnparentAndQueueFree();
		}
		else {
			Name = $"{GetType().Name}_inactive";
		}
	}

	/// <summary>
	/// Callback method for when the Behaviour is started.
	/// </summary>
	/// <param name="previousBehaviour">The last behaviour to be active before this one.</param>
	protected virtual void _Start(T? previousBehaviour) { }
	/// <summary>
	/// Callback method for when the Behaviour is stopped.
	/// </summary>
	/// <param name="nextBehaviour">The next behaviour to be activated after this one.</param>
	protected virtual void _Stop(T? nextBehaviour) { }
}