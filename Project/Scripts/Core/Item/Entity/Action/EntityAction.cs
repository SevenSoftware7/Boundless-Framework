namespace LandlessSkies.Core;

using System;
using Godot;
using SevenDev.Utility;


public abstract partial class EntityAction : Node {
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
		OnStart?.Invoke();
		_Start();
	}
	public void Stop() {
		OnStop?.Invoke();
		_Stop();

		this.UnparentAndQueueFree();
	}

	protected abstract void _Start();
	protected abstract void _Stop();


	public override void _Notification(int what) {
		base._Notification(what);
		if (what == NotificationPredelete) {
		}
	}
}