namespace LandlessSkies.Core;

public sealed class SlashAttackBuilder : AttackBuilder {
	public static readonly SlashAttackBuilder Instance = new();

	public override SlashAttack Build(Entity entity, SingleWeapon weapon) => new(entity, weapon);
}