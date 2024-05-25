namespace LandlessSkies.Core;

public class AttackActionInfo(SingleWeapon weapon, AttackBuilder builder) : EntityActionInfo {
	public SingleWeapon Weapon => weapon;
	protected override EntityAction Build(Entity entity) => builder.Build(entity, weapon);
}