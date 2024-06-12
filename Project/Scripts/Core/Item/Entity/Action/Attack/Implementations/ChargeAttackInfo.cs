namespace LandlessSkies.Core;

public abstract class ChargeAttackInfo : AttackInfo {
	protected internal abstract override ChargeAttack Create(Entity entity, SingleWeapon weapon);
}