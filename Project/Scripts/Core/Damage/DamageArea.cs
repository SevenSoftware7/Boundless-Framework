namespace LandlessSkies.Core;

using System.Collections.Generic;
using Godot;
using SevenDev.Utility;

[GlobalClass]
public partial class DamageArea : Area3D {
	private readonly List<IDamageable> hitBuffer = [];
	public IDamageDealer? DamageDealer;

	[Export] public float Damage = 1f;
	public TimeDuration? LifeTime;
	[Export] public bool SelfDamage = false;

	[Signal] public delegate void OnDestroyEventHandler();


	public DamageArea() : base() {
		CollisionLayer = CollisionLayers.Damage;
		CollisionMask = CollisionLayers.Damage | CollisionLayers.Entity;
	}
	public DamageArea(ulong lifeTime) : this() {
		if (lifeTime != 0) {
			LifeTime = new(true, lifeTime);
		}
	}


	public DamageArea(IDamageDealer? damageDealer, float damage = 1f, ulong lifeTime = 0, bool selfDamage = false) : this(lifeTime) {
		DamageDealer = damageDealer;
		Damage = damage;
		SelfDamage = selfDamage;
	}


	private void OnBodyEntered(Node3D body) {
		if (body is IDamageable damageable && (damageable != DamageDealer || SelfDamage)) {
			if (hitBuffer.Count == 0) {
				Callable.From(ApplyBufferedDamage).CallDeferred();
			}
			hitBuffer.Add(damageable);
		}
		else if (body is DamageArea damageArea) {
			Parry(damageArea);
			damageArea.Parry(this);
		}
	}

	public void Parry(DamageArea other) {
		if (DamageDealer is IDamageable targetDealer) {
			other.hitBuffer.Remove(targetDealer);
		}
	}

	private void ApplyBufferedDamage() {
		foreach (IDamageable hit in hitBuffer) {
			ApplyDamage(hit);
		}
		hitBuffer.Clear();
	}

	protected virtual void ApplyDamage(IDamageable hit) {
		hit.Damage(Damage);
	}


	public override void _Process(double delta) {
		base._Process(delta);
		if (LifeTime?.HasPassed ?? false) {
			EmitSignal(SignalName.OnDestroy);
			QueueFree();
		}
	}

	public override void _Ready() {
		base._Ready();
		BodyEntered += OnBodyEntered;
	}
}