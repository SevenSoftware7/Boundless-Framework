namespace LandlessSkies.Core;

using System;
using Godot;

public abstract partial class EntityAction : Node {
	private bool isStarted = false;
	public readonly SevenDev.Utility.Timer Lifetime = new();

	[Export] public Entity Entity;
	public event Action? OnStart;
	public event Action? OnStop;

	public abstract bool IsCancellable { get; }
	public abstract bool IsKnockable { get; }


	public EntityAction(Entity entity) {
		Entity = entity;
	}

	public void Start() {
		if (isStarted) return;

		OnStart?.Invoke();
		_Start();

		isStarted = true;
	}
	public void Stop() {
		OnStop?.Invoke();
		_Stop();

		if (! IsQueuedForDeletion()) {
			QueueFree();
		}
	}

	protected abstract void _Start();
	protected abstract void _Stop();


	public override void _Notification(int what) {
		base._Notification(what);
		if (what == NotificationPredelete) {
			Stop();
		}
	}
}