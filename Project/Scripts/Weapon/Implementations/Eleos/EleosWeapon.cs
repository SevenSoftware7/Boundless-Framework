using System;
using System.Collections.Generic;
using Godot;

namespace LandlessSkies.Core;

public partial class EleosWeapon(EleosWeaponData data, WeaponCostume? costume, Node3D root) : SimpleWeapon(data, costume, root) {
    private Attacks attacks;


    protected override void InitializeAttacks() {
        base.InitializeAttacks();
        attacks = new(this);
    }

    public override IEnumerable<IAttack.Info> GetAttacks(Entity target) {
        return [
            attacks.slashAttack,
        ];
    }



    private readonly struct Attacks(EleosWeapon weapon) {
        public readonly SlashAttack.Info slashAttack = new(weapon);
        
    }
}