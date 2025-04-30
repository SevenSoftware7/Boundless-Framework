namespace LandlessSkies.Core;

using System.Collections.Generic;
using Godot;

public sealed partial class SlashAttack : Attack {
	private static IEnumerable<TraitModifier> GetInnateTraits() => [
		new MultiplicativeModifier(Traits.GenericTurnSpeed, 0.175f),
		// new MultiplicativeModifier(Traits.GenericMoveSpeed, 0.175f),
		// new MultiplicativeModifier(Traits.GenericGravity, 0.025f),
	];


	public SlashAttack(Entity entity, Builder builder, Weapon weapon) : base(entity, builder, weapon, new(weapon.LibraryName, "Slash"), GetInnateTraits()) {}


	protected override void _Start() {
		base._Start();
		GD.Print($"{Weapon.DisplayName} Slash Attack (from {Entity.Name})");
	}



	public new sealed class Builder(Weapon weapon) : Attack.Builder(weapon) {
		public override SlashAttack Build(Entity entity) => new(entity, this, Weapon);
	}
}