using System;

namespace LandlessSkies.Core;

public interface IDamageDealer {
	public virtual IDamageable? Damageable => null;

	public void AwardDamage(float amount, DamageType type, IDamageable target);


	[Flags]
	public enum DamageType {
		Physical = 1 << 0,
		Magical = 1 << 1,
		Parry = 1 << 2,
	}
}