namespace LandlessSkies.Core;

using System.Collections.Generic;
using Godot;
using SevenDev.Utility;

[GlobalClass]
public partial class DamageArea3D : Area3D {
	private readonly List<IDamageable> hitBuffer = [];
	public IDamageDealer? DamageDealer;
	[Export] public float Damage = 1f;
	public TimeDuration? LifeTime;
	[Export] public bool SelfDamage = false;


	public DamageArea3D() : base() {
		CollisionLayer = Collisions.Damage;
		CollisionMask = Collisions.Damage | Collisions.Entity;
	}
	public DamageArea3D(ulong lifeTime) : this() {
		if (lifeTime != 0) {
			LifeTime = new(lifeTime);
		}
	}
	public DamageArea3D(IDamageDealer? damageDealer, float damage = 1f, ulong lifeTime = 0, bool selfDamage = false) : this(lifeTime) {
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
		else if (body is DamageArea3D damageArea) {
			Parry(damageArea);
			damageArea.Parry(this);
		}
	}

	public void Parry(DamageArea3D other) {
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
		if (LifeTime is null) return;


		if (LifeTime.IsDone) {
			QueueFree();
		}
	}

	public override void _Ready() {
		base._Ready();
		BodyEntered += OnBodyEntered;
	}
}