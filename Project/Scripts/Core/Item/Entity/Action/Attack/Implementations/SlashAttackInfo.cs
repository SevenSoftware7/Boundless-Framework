namespace LandlessSkies.Core;

using Godot;

public sealed class SlashAttackInfo : AttackInfo {
	public static readonly SlashAttackInfo Instance = new();

	private SlashAttackInfo() : base() { }

	protected internal override SlashAttack Create(Entity entity, Weapon weapon) => new(entity, weapon);
}