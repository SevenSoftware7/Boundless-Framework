using System;
using Godot;

namespace LandlessSkies.Core;

public sealed class SlashAttack : AttackAction {
	public override bool IsCancellable => false;
	public override bool IsKnockable => true;



	public SlashAttack() {
		GD.Print("Slash Attack");
		Dispose();
	}



	public record struct Info(SingleWeapon Weapon) : IInfo {
		public SingleWeapon Weapon { get; } = Weapon;
		public readonly float PotentialDamage => 2f;
		public readonly AttackType Type => AttackType.Melee | AttackType.Parry;

		public Action? BeforeExecute { get; set; }
		public Action? AfterExecute { get; set; }



		public readonly AttackAction Build() =>
			new SlashAttack();
	}
}