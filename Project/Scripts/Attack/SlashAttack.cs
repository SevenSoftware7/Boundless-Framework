using System;
using Godot;

namespace LandlessSkies.Core;

public class SlashAttack : IAttack {

    public void Execute() {
        GD.Print("Slash Attack");
    }



    public class Info(Weapon weapon) : IAttack.Info(weapon) {
        public override float PotentialDamage => 2f;

        public override IAttack Build() {
            return new SlashAttack();
        }
    }
}