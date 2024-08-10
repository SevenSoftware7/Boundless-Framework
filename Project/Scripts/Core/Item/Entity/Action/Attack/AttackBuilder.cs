namespace LandlessSkies.Core;

using Godot;

public sealed class AttackBuilder(AttackInfo info, Weapon weapon, StringName library) : ActionBuilder() {
	public readonly AttackInfo Info = info;
	public readonly Weapon Weapon = weapon;
	public readonly StringName Library = library;

	protected internal override Attack Create(Entity entity) => Info.Create(entity, Weapon, Library);
}