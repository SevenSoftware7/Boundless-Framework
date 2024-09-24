namespace LandlessSkies.Core;

public interface IDamageDealerProxy : IDamageDealer {
	public IDamageDealer? Sender { get; }

	IDamageable? IDamageDealer.Damageable => Sender?.Damageable;

	void IDamageDealer.AwardDamage(in DamageData data, IDamageable? target) {
		AwardDamage(in data, target);
		Sender?.AwardDamage(in data, target);
	}
	public new void AwardDamage(in DamageData data, IDamageable? target) { }
}