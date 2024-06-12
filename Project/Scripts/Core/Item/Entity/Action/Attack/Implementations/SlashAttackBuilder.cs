namespace LandlessSkies.Core;

public sealed class SlashAttackInfo : AttackInfo {
	public static readonly SlashAttackInfo Instance = new();

	protected internal override SlashAttack Create(Entity entity, SingleWeapon weapon) => new(entity, weapon);
}