namespace LandlessSkies.Core;

using Godot;

public sealed partial class SlashAttack(Entity entity, Weapon weapon) : Attack(entity, weapon, new(weapon.LibraryName, "Slash"), [slowRotation/* , slowMovement */, lowGravity]) {
	private static readonly AttributeModifier slowRotation = new MultiplicativeModifier(Attributes.GenericTurnSpeed, 0.175f);
	// private static readonly AttributeModifier slowMovement = new MultiplicativeModifier(Attributes.GenericMoveSpeed, 0.175f);
	private static readonly AttributeModifier lowGravity = new MultiplicativeModifier(Attributes.GenericGravity, 0.025f);

	protected override void _Start() {
		base._Start();
		GD.Print($"{Weapon.DisplayName} Slash Attack (from {Entity.Name})");
	}



	public new sealed class Builder(Weapon weapon) : Attack.Builder(weapon) {
		public override SlashAttack Build(Entity entity) => new(entity, Weapon);
	}
}