namespace LandlessSkies.Core;

using System;
using Godot;

public abstract partial class EntityAction : Node, IPlayerHandler {
	public event Action? OnDestroy;


	public abstract bool IsCancellable { get; }
	public abstract bool IsKnockable { get; }



	public virtual void HandlePlayer(Player player) { }


	public override void _Notification(int what) {
		base._Notification(what);
		if (what == NotificationPredelete) {
			OnDestroy?.Invoke();
		}
	}
}