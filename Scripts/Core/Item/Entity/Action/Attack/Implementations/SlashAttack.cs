namespace LandlessSkies.Core;

using Godot;

public sealed partial class SlashAttack(Entity entity, Weapon weapon) : Attack(entity, weapon, new(weapon.LibraryName, "Slash"), [slowRotation/* , slowMovement *//* , lowGravity */]) {
	private static readonly TraitModifier slowRotation = new MultiplicativeModifier(Traits.GenericTurnSpeed, 0.175f);
	// private static readonly TraitModifier slowMovement = new MultiplicativeModifier(Traits.GenericMoveSpeed, 0.175f);
	private static readonly TraitModifier lowGravity = new MultiplicativeModifier(Traits.GenericGravity, 0.025f);

	protected override void _Start() {
		base._Start();
		GD.Print($"{Weapon.DisplayName} Slash Attack (from {Entity.Name})");
	}



	public new sealed class Builder(Weapon weapon) : Attack.Builder(weapon) {
		public override SlashAttack Build(Entity entity) => new(entity, Weapon);
	}
}