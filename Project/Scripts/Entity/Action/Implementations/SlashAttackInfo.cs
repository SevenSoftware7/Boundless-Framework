namespace LandlessSkies.Core;

public sealed record class SlashAttackInfo(SingleWeapon Weapon) : AttackInfo(Weapon) {
	public override SlashAttack Build() =>
		new(Weapon);
}