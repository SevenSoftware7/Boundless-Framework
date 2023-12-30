

using System;
using System.Collections.Generic;
using Godot;

namespace LandlessSkies.Core;

public abstract class AttackAction : EntityAction {

	public interface IAttackInfo : IInfo {
		public Weapon Weapon { get; init; }

		float PotentialDamage { get; }
		AttackType Type { get; }



		public new AttackAction Build();
		EntityAction IInfo.Build() => Build();

		
		public static IAttackInfo SelectAttack(IAttackInfo[] attacks, IComparer<IAttackInfo> priority, float skillLevel = 0.5f) {
			skillLevel = Mathf.Clamp(skillLevel, 0, 1);

			Array.Sort(attacks, priority);

			float skillMargin = skillLevel * attacks.Length;
			int weightedIndex = Mathf.RoundToInt(GD.Randf() * skillMargin);

			return attacks[weightedIndex];
		}



		public static class Comparers {
			public readonly static IComparer<IAttackInfo> PureDamage = new ComparisonComparer(
				(a, b) => a?.PotentialDamage.CompareTo(b?.PotentialDamage ?? 0) ?? 0
			);



			private class ComparisonComparer(Comparison<IAttackInfo?> Comparison) : IComparer<IAttackInfo> {
				public int Compare(IAttackInfo? x, IAttackInfo? y) {
					return Comparison(x, y);
				}
			}
		}
	}
	

	[Flags]
	public enum AttackType : byte {
		Melee = 1 << 0,
		Projectile = 1 << 1,
		Hitscan = 1 << 2,
		Parry = 1 << 3,
	}
}