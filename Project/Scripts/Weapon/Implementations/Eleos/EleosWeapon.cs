using System;
using System.Collections.Generic;
using Godot;

namespace LandlessSkies.Core;

public partial class EleosWeapon : SimpleWeapon {

    public EleosWeapon() : base() {}
    public EleosWeapon(EleosWeaponData data, WeaponCostume? costume, Node3D root) : base(data, costume, root) {}


    public override IEnumerable<IAttack.AttackInfo> GetAttacks(Entity target) {
        return [
            // new IAttack.AttackInfo()
        ];
    }
}