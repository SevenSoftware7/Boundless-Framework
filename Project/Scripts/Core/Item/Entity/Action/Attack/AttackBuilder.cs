namespace LandlessSkies.Core;

public sealed class AttackBuilder(AttackInfo info, SingleWeapon weapon) : EntityActionBuilder() {
	public readonly AttackInfo Info = info;
	public readonly SingleWeapon Weapon = weapon;

	protected internal override Attack Build(Entity entity) => Info.Create(entity, Weapon);
}