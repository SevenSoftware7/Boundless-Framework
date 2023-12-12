using System;
using System.Collections.Generic;
using Godot;

namespace LandlessSkies.Core;

public interface IAttack {
    
    void Execute();
    

    public readonly struct AttackInfo(Func<IAttack> builder) {
        public required readonly float PotentialDamage { get; init; }


        public void Build() => builder.Invoke();

        public static AttackInfo SelectAttack(AttackInfo[] attacks, float skillLevel = 0.5f) {
            skillLevel = Mathf.Clamp(skillLevel, 0, 1);

            IComparer<AttackInfo> priority = Comparers.PureDamage;
            Array.Sort(attacks, priority);

            float skillMargin = skillLevel * attacks.Length;
            int weightedIndex = Mathf.RoundToInt(GD.Randf() * skillMargin);

            return attacks[weightedIndex];
        }



        public static class Comparers {
            public readonly static IComparer<AttackInfo> PureDamage = new ComparisonComparer<AttackInfo>((a, b) => a.PotentialDamage.CompareTo(b.PotentialDamage));

            private class ComparisonComparer<T>(Comparison<T?> Comparison) : IComparer<T> where T : notnull {
                public int Compare(T? x, T? y) {
                    return Comparison(x, y);
                }
            }
        }
    }
}