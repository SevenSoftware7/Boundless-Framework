namespace LandlessSkies.Core;

using System;
using Godot;


public abstract record class AttackInfo(SingleWeapon Weapon) : EntityActionInfo() {
	// [Export] public SingleWeapon Weapon { get; private set; } = Weapon;

	public float PotentialDamage { get; }
	public Attack.AttackType Type { get; }


	public abstract override Attack Build();

	// public virtual AttackActionInfo ForWeapon(SingleWeapon weapon) {
	// 	AttackActionInfo duplicate = (AttackActionInfo)Duplicate();
	// 	duplicate.Weapon = weapon;

	// 	return duplicate;
	// }
}