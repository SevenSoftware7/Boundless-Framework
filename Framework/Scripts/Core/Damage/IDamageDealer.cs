using System;

namespace SevenDev.Boundless;

public interface IDamageDealer {
	public virtual IDamageable? Damageable => null;

	public void AwardDamage(in DamageData data, IDamageable? target);


	[Flags]
	public enum DamageFlags {
		Physical = 1 << 0,
		Magical = 1 << 1,
		CanParry = 1 << 2,
		CanBeParried = 1 << 3,
		SelfDamage = 1 << 4,
	}
}