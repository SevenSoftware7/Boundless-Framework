namespace LandlessSkies.Core;

public abstract class AttackBuilder {
	public float PotentialDamage { get; }
	public Attack.AttackType Type { get; }


	public AttackActionInfo GetInfo(SingleWeapon weapon) => new(weapon, this);
	public abstract Attack Build(SingleWeapon weapon);
}