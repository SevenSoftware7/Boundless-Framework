using System;
using System.Collections.Generic;
using Godot;

namespace LandlessSkies.Core;

public abstract class AttackAction : EntityAction {

	public new interface IInfo : EntityAction.IInfo {
		public SingleWeapon Weapon { get; }

		float PotentialDamage { get; }
		AttackType Type { get; }



		public new AttackAction Build();
		EntityAction EntityAction.IInfo.Build() => Build();
	}


	[Flags]
	public enum AttackType : byte {
		Melee = 1 << 0,
		Projectile = 1 << 1,
		Hitscan = 1 << 2,
		Parry = 1 << 3,
		Explosive = 1 << 4,
	}
}

public static class AttackExtensions {
	public static AttackAction.IInfo SelectAttack(this AttackAction.IInfo[] attacks, IComparer<AttackAction.IInfo> priority, float skillLevel = 0.5f) {
		skillLevel = Mathf.Clamp(skillLevel, 0, 1);

		Array.Sort(attacks, priority);

		float skillMargin = skillLevel * attacks.Length;
		int weightedIndex = Mathf.RoundToInt(GD.Randf() * skillMargin);

		return attacks[weightedIndex];
	}



	public static class Comparers {
		public readonly static IComparer<AttackAction.IInfo> PureDamage = new ComparisonComparer(
			(a, b) => a?.PotentialDamage.CompareTo(b?.PotentialDamage ?? 0) ?? 0
		);



		private class ComparisonComparer(Comparison<AttackAction.IInfo?> Comparison) : IComparer<AttackAction.IInfo> {
			public int Compare(AttackAction.IInfo? x, AttackAction.IInfo? y) {
				return Comparison(x, y);
			}
		}
	}
}