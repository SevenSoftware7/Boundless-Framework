namespace LandlessSkies.Core;

public interface IDamageDealerProxy : IDamageDealer {
	public IDamageDealer? Sender { get; }

	IDamageable? IDamageDealer.Damageable => Sender?.Damageable;

	void IDamageDealer.AwardDamage(float amount, DamageType type, IDamageable target) {
		AwardDamage(ref amount, ref type, target);
		Sender?.AwardDamage(amount, type, target);
	}
	public virtual void AwardDamage(ref float amount, ref DamageType type, IDamageable target) { }
}