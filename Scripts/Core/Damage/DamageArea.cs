namespace Seven.Boundless;

using System.Collections.Generic;
using System.Runtime.InteropServices;
using Godot;
using Seven.Boundless.Utility;

[GlobalClass]
public partial class DamageArea : Area3D {
	private readonly Dictionary<IDamageable, int> hitHistory = [];
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
	[Export] public uint MaxImpacts = 0; // 0 means no limit

	[Signal] public delegate void DestroyedEventHandler();


	public IDamageDealer? DamageDealer { get; init; }



	public DamageArea() : base() {
		CollisionLayer = CollisionLayers.Damage;
		CollisionMask = CollisionLayers.Damage | CollisionLayers.Entity | CollisionLayers.Prop;
	}



	private void ApplyBufferedDamage() {
		DamageData damageData = new(Damage, DamageData.DamageType.Standard);

		foreach (IDamageable target in hitBuffer) {
			damageData.Inflict(target, DamageDealer);
		}
		hitBuffer.Clear();
	}

	protected void HandleHit(IDamageable damageable) {
		if (damageable == DamageDealer?.Damageable && !Flags.HasFlag(IDamageDealer.DamageFlags.SelfDamage)) return;

		if (MaxImpacts != 0) {
			ref int hitCount = ref CollectionsMarshal.GetValueRefOrAddDefault(hitHistory, damageable, out _);
			if (hitCount >= MaxImpacts) {
				// GD.Print($"DamageArea {Name} has reached max impacts for {damageable.Name}, ignoring further hits.");
				return;
			}
			hitCount++;
		}

		if (hitBuffer.Count == 0) {
			Callable.From(ApplyBufferedDamage).CallDeferred();
		}
		hitBuffer.Add(damageable);
	}

	protected void HandleClash(DamageArea other) {
		if (!Flags.HasFlag(IDamageDealer.DamageFlags.CanParry) || !other.Flags.HasFlag(IDamageDealer.DamageFlags.CanBeParried)) return;

		if (DamageDealer?.Damageable is IDamageable selfDamageable) {
			other.hitBuffer.Remove(selfDamageable);
		}

		DamageData parryData = new(Damage, DamageData.DamageType.Parry);
		parryData.Inflict(other.DamageDealer?.Damageable, DamageDealer);
	}

	private void _BodyEntered(Node3D body) {
		// GD.Print($"Body {body.Name} collided with DamageArea {Name} (from {DamageDealer?.GetType().Name})");
		if (body is not IDamageable damageable) return;

		HandleHit(damageable);
	}
	private void _AreaEntered(Node3D area) {
		// GD.Print($"Area {area.Name} collided with DamageArea {Name} (from {DamageDealer?.GetType().Name})");
		if (area is not DamageArea damageArea) return;

		HandleClash(damageArea);
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