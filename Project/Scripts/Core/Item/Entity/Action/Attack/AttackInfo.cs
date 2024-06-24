namespace LandlessSkies.Core;

using Godot;

public abstract class AttackInfo() {
	public float PotentialDamage { get; }
	public Attack.AttackType Type { get; }

	protected internal abstract Attack Create(Entity entity, Weapon weapon, StringName library);
}