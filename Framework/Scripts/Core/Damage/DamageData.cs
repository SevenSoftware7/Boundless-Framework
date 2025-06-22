using System;

namespace SevenDev.Boundless;

public ref struct DamageData {
	public float Amount;
	public DamageType Type;


	public DamageData(float amount, DamageType type) {
		Amount = amount;
		Type = type;
	}


	public enum DamageType {
		Standard,
		Status,
		Parry,
	}
}

public static class DamageDataExtensions {
	public static void Inflict(this DamageData data, IDamageable? target, IDamageDealer? origin) {
		target?.Damage(ref data);
		origin?.AwardDamage(in data, target);
	}
}