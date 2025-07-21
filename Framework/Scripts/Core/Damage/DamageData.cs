using System;

namespace SevenDev.Boundless;

public ref struct DamageData {
	public float Amount;
	public DamageType Type;


	public DamageData(float amount, DamageType type) {
		Amount = amount;
		Type = type;
	}

	[Flags]
	public enum DamageType : byte {
		Standard = 1 << 0,
		Status = 1 << 1,
		Parry = 1 << 2,
	}
}

public static class DamageDataExtensions {
	public static void Inflict(this DamageData data, IDamageable? target, IDamageDealer? origin) {
		target?.Damage(ref data);
		origin?.AwardDamage(in data, target);
	}
}