namespace LandlessSkies.Core;

public abstract class AttackInfo() {
	public float PotentialDamage { get; }
	public Attack.AttackType Type { get; }

	protected internal abstract Attack Create(Entity entity, SingleWeapon weapon);
}