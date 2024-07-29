namespace LandlessSkies.Core;

using Godot;

public abstract class ChargeAttackInfo : AttackInfo {
	protected internal abstract override ChargeAttack Create(Entity entity, Weapon weapon, StringName library);
}