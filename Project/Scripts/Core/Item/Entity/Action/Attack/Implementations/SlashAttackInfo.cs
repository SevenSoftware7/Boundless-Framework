using Godot;

namespace LandlessSkies.Core;

public sealed class SlashAttackInfo : AttackInfo {
	public static readonly SlashAttackInfo Instance = new();

	private SlashAttackInfo() : base() { }

	protected internal override SlashAttack Create(Entity entity, SingleWeapon weapon, StringName library) => new(entity, weapon, library);
}