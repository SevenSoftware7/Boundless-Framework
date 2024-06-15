namespace LandlessSkies.Core;

using Godot;

public sealed partial class SlashAttack(Entity entity, SingleWeapon weapon, StringName library) : AnimationAttack(entity, weapon, library) {
	private static readonly StringName Attack = "Slash";

	protected override StringName AnimationName => Attack;


	protected override void _Start() {
		base._Start();
		GD.Print($"{Weapon.DisplayName} Slash Attack");
	}
}