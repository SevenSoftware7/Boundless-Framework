namespace Seven.Boundless;

using Godot;
using Godot.Collections;
using Seven.Boundless.Utility;


[Tool]
[GlobalClass]
public sealed partial class DamageAreaBuilder : Resource {
	[Export] public ulong LifeTime = 250;

	[Export] public float Damage = 1f;
	[Export] public IDamageDealer.DamageFlags DamageType = IDamageDealer.DamageFlags.Physical;
	[Export] public uint MaxImpacts = 0;

	[Export] public Array<DamageHitboxBuilder> HitboxBuilders = [];


	public DamageArea Build(IDamageDealer damageDealer, Node? parent = null) {
		DamageArea damageArea = new() {
			DamageDealer = damageDealer,
			LifeTime = LifeTime,
			Damage = Damage,
			Flags = DamageType,
			MaxImpacts = MaxImpacts,
		};
		if (parent is not null) {
			damageArea.ParentTo(parent);
		}

		foreach (DamageHitboxBuilder hitboxBuilder in HitboxBuilders) {
			hitboxBuilder.Build(damageArea);
		}

		return damageArea;
	}
}