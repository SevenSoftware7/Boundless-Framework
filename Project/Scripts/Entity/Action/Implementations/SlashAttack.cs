namespace LandlessSkies.Core;

using System;
using Godot;

public sealed class SlashAttack : AttackAction {
	public static readonly Info DefaultInfo = new();
	public override bool IsCancellable => false;
	public override bool IsKnockable => true;



	public SlashAttack() {
		GD.Print("Slash Attack");
		Dispose();
	}



	public struct Info : IInfo {
		public SingleWeapon? Weapon { get; init; }
		public readonly float PotentialDamage { get; } = 1f;
		public readonly AttackType Type { get; } = AttackType.Melee | AttackType.Parry;

		public Action? BeforeExecute { get; set; }
		public Action? AfterExecute { get; set; }

		public Info() {}

		public readonly AttackAction Build() =>
			new SlashAttack();
	}
}