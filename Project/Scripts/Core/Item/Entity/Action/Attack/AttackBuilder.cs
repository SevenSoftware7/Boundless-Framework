namespace LandlessSkies.Core;

using Godot;

public sealed class AttackBuilder(AttackInfo info, Weapon weapon) : ActionBuilder() {
	public readonly AttackInfo Info = info;
	public readonly Weapon Weapon = weapon;

	protected internal override Attack Create(Entity entity) => Info.Create(entity, Weapon);
}