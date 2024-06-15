namespace LandlessSkies.Core;

using Godot;

public sealed class AttackBuilder(AttackInfo info, SingleWeapon weapon, StringName library) : EntityActionBuilder() {
	public readonly AttackInfo Info = info;
	public readonly SingleWeapon Weapon = weapon;
	public readonly StringName Library = library;

	protected internal override Attack Create(Entity entity) => Info.Create(entity, Weapon, Library);
}