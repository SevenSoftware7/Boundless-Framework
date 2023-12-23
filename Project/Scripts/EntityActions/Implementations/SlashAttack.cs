using System;
using Godot;

namespace LandlessSkies.Core;

public class SlashAttack : AttackAction {

	public override bool IsCancellable => false;
	public override bool IsKnockable => true;



	public SlashAttack() {
		GD.Print("Slash Attack");
		Dispose();
	}



	public new class Info(Weapon weapon) : AttackAction.Info(weapon) {
		public override float PotentialDamage => 2f;
		public override AttackType Type => AttackType.Melee | AttackType.Parry;

		protected override SlashAttack Build() {
			return new SlashAttack();
		}
	}
}