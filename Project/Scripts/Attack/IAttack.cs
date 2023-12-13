using System;
using System.Collections.Generic;
using Godot;

namespace LandlessSkies.Core;

public interface IAttack/*  : IDisposable */ {
    
    void Execute();



    public abstract class Info(Weapon weapon) {
        public readonly Weapon Weapon = weapon;

        public abstract float PotentialDamage { get; } 


        public abstract IAttack Build();



        public static Info SelectAttack(Info[] attacks, float skillLevel = 0.5f) {
            skillLevel = Mathf.Clamp(skillLevel, 0, 1);

            IComparer<Info> priority = Comparers.PureDamage;
            Array.Sort(attacks, priority);

            float skillMargin = skillLevel * attacks.Length;
            int weightedIndex = Mathf.RoundToInt(GD.Randf() * skillMargin);

            return attacks[weightedIndex];
        }



        public static class Comparers {
            public readonly static IComparer<Info> PureDamage = new ComparisonComparer<Info>((a, b) => a?.PotentialDamage.CompareTo(b?.PotentialDamage ?? 0) ?? 0);



            private class ComparisonComparer<T>(Comparison<T?> Comparison) : IComparer<T> where T : notnull {
                public int Compare(T? x, T? y) {
                    return Comparison(x, y);
                }
            }
        }

    }
}