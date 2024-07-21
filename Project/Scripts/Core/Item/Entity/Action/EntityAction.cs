namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using Godot;
using SevenDev.Utility;


public abstract partial class EntityAction : Node {
	public readonly SevenDev.Utility.Timer Lifetime = new(false);

	[Export] public Entity Entity;
	public IEnumerable<AttributeModifier> Modifiers;

	public event Action? OnStart;
	public event Action? OnStop;

	public EntityAction(Entity entity, IEnumerable<AttributeModifier>? modifiers = null) {
		Entity = entity;
		Modifiers = modifiers ?? [];
	}

	public abstract bool IsCancellable { get; }
	public abstract bool IsInterruptable { get; }
	public void Start() {
		if (Entity is null) {
			GD.PushError($"Entity was null when trying to execute {GetType().Name} Action");
			return;
		}
		Lifetime.Start();
		OnStart?.Invoke();

		Entity.AttributeModifiers.AddRange(Modifiers);
		_Start();
	}
	public void Stop() {
		OnStop?.Invoke();

		Entity.AttributeModifiers.RemoveRange(Modifiers);
		_Stop();

		this.UnparentAndQueueFree();
	}

	protected virtual void _Start() { }

	protected virtual void _Stop() { }
}