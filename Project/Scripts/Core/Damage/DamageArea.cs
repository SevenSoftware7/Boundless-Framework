namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using Godot;
using SevenDev.Utility;

[GlobalClass]
public partial class DamageArea : Area3D {
	private readonly List<IDamageable> hitBuffer = [];

	[Export] public ulong LifeTime {
		get => _lifeTime?.DurationMsec ?? 0;
		set {
			if (_lifeTime is null) {
				_lifeTime = new(true, value);
			}
			else {
				_lifeTime.DurationMsec = value;
			}
		}
	}
	public TimeDuration? _lifeTime;

	[Export] public float Damage = 1f;
	[Export] public DamageType Type = DamageType.Physical;

	[Export] public bool SelfDamage = false;

	[Export] public bool CanParry = false;
	[Export] public bool Parriable = false;

	[Signal] public delegate void OnDestroyEventHandler();


	public IDamageDealer? DamageDealer { get; init; }



	public DamageArea() : base() {
		CollisionLayer = CollisionLayers.Damage;
		CollisionMask = CollisionLayers.Damage | CollisionLayers.Entity | CollisionLayers.Prop;
	}



	public void Parry(DamageArea other) {
		if (!CanParry || !other.Parriable) return;

		if (DamageDealer?.Damageable is IDamageable damageable) {
			other.hitBuffer.Remove(damageable);
			DamageDealer.AwardDamage(other.Damage, (IDamageDealer.DamageType)Type | IDamageDealer.DamageType.Parry, damageable);
		}
	}

	private void ApplyBufferedDamage() {
		foreach (IDamageable hit in hitBuffer) {
			hit.Damage(Damage);
			DamageDealer?.AwardDamage(Damage, (IDamageDealer.DamageType)Type, hit);
		}
		hitBuffer.Clear();
	}



	private void _BodyEntered(Node3D body) {
		// GD.Print($"Body {body.Name} collided with DamageArea {Name} (from {DamageDealer?.GetType().Name})");
		if (body is IDamageable damageable && (SelfDamage || damageable != DamageDealer?.Damageable)) {
			if (hitBuffer.Count == 0) {
				Callable.From(ApplyBufferedDamage).CallDeferred();
			}
			hitBuffer.Add(damageable);
		}
	}
	private void _AreaEntered(Node3D area) {
		// GD.Print($"Area {area.Name} collided with DamageArea {Name} (from {DamageDealer?.GetType().Name})");
		if (area is DamageArea damageArea) {
			Parry(damageArea);
		}
	}


	public override void _Process(double delta) {
		base._Process(delta);
		if (_lifeTime is not null && _lifeTime.HasPassed) {
			QueueFree();
		}
	}

	public override void _Ready() {
		base._Ready();
		BodyEntered += _BodyEntered;
		AreaEntered += _AreaEntered;
	}

	public override void _Notification(int what) {
		base._Notification(what);
		switch((ulong)what) {
			case NotificationPredelete:
				ApplyBufferedDamage();
				EmitSignal(SignalName.OnDestroy);
				break;
		}
	}



	[Flags]
	public enum DamageType {
		Physical = IDamageDealer.DamageType.Physical,
		Magical = IDamageDealer.DamageType.Magical,
	}
}