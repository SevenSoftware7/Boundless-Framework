using System;
using System.Collections.Generic;
using Godot;

namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class EleosWeapon : SimpleWeapon {
    private Attacks attacks;


    public EleosWeapon() : base() {}
    public EleosWeapon(EleosWeaponData data, WeaponCostume? costume, Node3D root) : base(data, costume, root) {}


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