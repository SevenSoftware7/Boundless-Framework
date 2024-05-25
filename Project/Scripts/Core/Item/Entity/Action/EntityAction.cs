namespace LandlessSkies.Core;

using System;
using Godot;

public abstract partial class EntityAction : Node {
	[Export] public Entity Entity;
	public event Action? OnDestroy;


	protected EntityAction() : this(null!) { }
	public EntityAction(Entity entity) {
		Entity = entity;
	}


	public abstract bool IsCancellable { get; }
	public abstract bool IsKnockable { get; }


	public override void _Notification(int what) {
		base._Notification(what);
		if (what == NotificationPredelete) {
			OnDestroy?.Invoke();
		}
	}
}