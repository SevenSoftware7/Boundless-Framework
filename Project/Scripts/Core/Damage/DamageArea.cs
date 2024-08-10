namespace LandlessSkies.Core;

using System.Collections.Generic;
using Godot;
using SevenDev.Utility;

[GlobalClass]
public partial class DamageArea : Area3D {
	private readonly List<IDamageable> hitBuffer = [];

	public TimeDuration? LifeTime;

	public IDamageDealer? DamageDealer { get; init; }
	[Export] public float Damage = 1f;
	[Export] public bool SelfDamage = false;

	[Export] public bool CanParry = false;
	[Export] public bool Parriable = false;

	[Signal] public delegate void OnDestroyEventHandler();



	public DamageArea() : base() {
		CollisionLayer = CollisionLayers.Damage;
		CollisionMask = CollisionLayers.Damage | CollisionLayers.Entity | CollisionLayers.Prop;
	}
	public DamageArea(ulong lifeTime) : this() {
		if (lifeTime != 0) {
			LifeTime = new(true, lifeTime);
		}
	}



	public void GetParriedBy(DamageArea other) {
		if (!Parriable) return;

		if (other.DamageDealer is IDamageable target) {
			hitBuffer.Remove(target);
		}
	}

	private void ApplyBufferedDamage() {
		foreach (IDamageable hit in hitBuffer) {
			hit.Damage(Damage);
		}
		hitBuffer.Clear();
	}



	private void _BodyEntered(Node3D body) {
		GD.Print($"Body {body.Name} collided with DamageArea {Name}");
		if (body is IDamageable damageable && (damageable != DamageDealer || SelfDamage)) {
			if (hitBuffer.Count == 0) {
				Callable.From(ApplyBufferedDamage).CallDeferred();
			}
			hitBuffer.Add(damageable);
		}
	}
	private void _AreaEntered(Node3D area) {
		GD.Print($"Area {area.Name} collided with DamageArea {Name}");
		if (area is DamageArea damageArea) {
			if (CanParry) damageArea.GetParriedBy(this);
		}
	}


	public override void _Process(double delta) {
		base._Process(delta);
		if (LifeTime is not null && LifeTime.HasPassed) {
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
}