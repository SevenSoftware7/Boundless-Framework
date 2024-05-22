namespace LandlessSkies.Core;

public abstract class ChargeAttackBuilder : AttackBuilder {
	public abstract override ChargeAttack Build(SingleWeapon weapon);
}