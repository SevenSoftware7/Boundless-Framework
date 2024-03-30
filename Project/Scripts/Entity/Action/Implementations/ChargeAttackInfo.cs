namespace LandlessSkies.Core;

public abstract record class ChargeAttackInfo(SingleWeapon Weapon) : AttackInfo(Weapon) {
	public abstract override ChargeAttack Build();
}