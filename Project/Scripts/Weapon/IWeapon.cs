
using System;
using Godot;
using LandlessSkies.Generators;


namespace LandlessSkies.Core;

[NodeInterface]
public interface IWeapon : IDataContainer<WeaponData>, ICostumable<WeaponCostume>, ILoadable, IInjectable<Skeleton3D?> {


    Type WeaponType { get; }
    Handedness WeaponHandedness { get; }


    // for each attack, evaluate the desirability, return one according to the "skill" level of the IA
    // e.g. if the IA has 0.5 skill, it will choose the attack with the highest desirability 50% of the time
    // if the IA has 1 skill, it will always choose the attack with the highest desirability
    // public int EvaluateAttacks(IActor attacker, IActor target, float skillMultiplier);

    

    [Flags]
    public enum Type : byte {
        Sparring = 1 << 0,
        OneHanded = 1 << 1,
        TwoHanded = 1 << 2,
        Polearm = 1 << 3,
        Dual = 1 << 4,
        Shield = 1 << 5,
        Dagger = 1 << 6,
        // = 1 << 7 /// Do not exceed 8 flags <see cref="byte"/>
    };

    [Flags]
    public enum Handedness : byte {
        Left = 1 << 0,
        Right = 1 << 1,
    }

}