namespace LandlessSkies.Core;

using Godot;

public abstract partial class DamageAreaBuilder<T> : Resource where T : IDamageDealer {
	[Export] public ulong LifeTime = 250;

	[Export] public float Damage = 1f;
	[Export] public IDamageDealer.DamageFlags DamageType = IDamageDealer.DamageFlags.Physical;

	[Export] public bool SelfDamage = false;
	[Export] public bool CanParry = false;
	[Export] public bool Parriable = false;

	[Export] public Godot.Collections.Array<DamageHitboxBuilder> HitboxBuilders = [];


	public DamageArea Build(T damageDealer) {
		DamageArea damageArea = new() {
			DamageDealer = damageDealer,
			LifeTime = LifeTime,
			Damage = Damage,
			Flags = DamageType,
		};

		SetupDamageArea(damageDealer, damageArea);

		foreach (DamageHitboxBuilder hitboxBuilder in HitboxBuilders) {
			hitboxBuilder.Build(damageArea);
		}

		return damageArea;
	}

	protected virtual void SetupDamageArea(T damageDealer, DamageArea area) { }
}