namespace SevenDev.Boundless;

using System.Collections.Generic;
using Godot;
using SevenDev.Boundless.Utility;

[GlobalClass]
public partial class DamageArea : Area3D {
	private readonly List<IDamageable> hitBuffer = [];

	[Export] public ulong LifeTime {
		get => _lifeTime?.DurationMsec ?? 0;
		set {
			if (_lifeTime is null) {
				_lifeTime = new(value, true);
			}
			else {
				_lifeTime.DurationMsec = value;
			}
		}
	}
	public Countdown? _lifeTime;

	[Export] public float Damage = 1f;
	[Export] public IDamageDealer.DamageFlags Flags = IDamageDealer.DamageFlags.Physical;

	[Signal] public delegate void DestroyedEventHandler();


	public IDamageDealer? DamageDealer { get; init; }



	public DamageArea() : base() {
		CollisionLayer = CollisionLayers.Damage;
		CollisionMask = CollisionLayers.Damage | CollisionLayers.Entity | CollisionLayers.Prop;
	}



	public void Parry(DamageArea other) {
		if (!Flags.HasFlag(IDamageDealer.DamageFlags.CanParry) || !other.Flags.HasFlag(IDamageDealer.DamageFlags.CanBeParried)) return;

		if (DamageDealer?.Damageable is IDamageable selfDamageable) {
			other.hitBuffer.Remove(selfDamageable);
		}

		DamageData parryData = new(Damage, DamageData.DamageType.Parry);
		parryData.Inflict(other.DamageDealer?.Damageable, DamageDealer);
	}

	private void ApplyBufferedDamage() {
		DamageData damageData = new(Damage, DamageData.DamageType.Standard);

		foreach (IDamageable target in hitBuffer) {
			damageData.Inflict(target, DamageDealer);
		}
		hitBuffer.Clear();
	}



	private void _BodyEntered(Node3D body) {
		// GD.Print($"Body {body.Name} collided with DamageArea {Name} (from {DamageDealer?.GetType().Name})");
		if (body is IDamageable damageable && (Flags.HasFlag(IDamageDealer.DamageFlags.SelfDamage) || damageable != DamageDealer?.Damageable)) {
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
		if (_lifeTime is not null && _lifeTime.IsCompleted) {
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
				EmitSignalDestroyed();
				break;
		}
	}
}