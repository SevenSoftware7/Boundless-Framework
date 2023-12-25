

using System;
using System.Collections.Generic;
using Godot;

namespace LandlessSkies.Core;

public abstract class AttackAction : EntityAction {

	public new abstract class Info(Weapon weapon) : EntityAction.Info() {
		public readonly Weapon Weapon = weapon;
		public abstract AttackType Type { get; }



		protected abstract override AttackAction Build();

		
		public static Info SelectAttack(Info[] attacks, IComparer<Info> priority, float skillLevel = 0.5f) {
			skillLevel = Mathf.Clamp(skillLevel, 0, 1);

			Array.Sort(attacks, priority);

			float skillMargin = skillLevel * attacks.Length;
			int weightedIndex = Mathf.RoundToInt(GD.Randf() * skillMargin);

			return attacks[weightedIndex];
		}

		[Flags]
		public enum AttackType : byte {
			Melee = 1 << 0,
			Projectile = 1 << 1,
			Hitscan = 1 << 2,
			Parry = 1 << 3,
		}



		public static class Comparers {
			public readonly static IComparer<Info> PureDamage = new ComparisonComparer<Info>(
				(a, b) => a?.PotentialDamage.CompareTo(b?.PotentialDamage ?? 0) ?? 0
			);



			private class ComparisonComparer<T>(Comparison<T?> Comparison) : IComparer<T> where T : notnull {
				public int Compare(T? x, T? y) {
					return Comparison(x, y);
				}
			}
		}
	}
}