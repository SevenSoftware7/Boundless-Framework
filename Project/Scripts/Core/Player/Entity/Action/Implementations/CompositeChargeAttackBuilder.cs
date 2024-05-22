namespace LandlessSkies.Core;

using Godot;

public class CompositeChargeAttackBuilder(AttackBuilder UnchargedAttack, AttackBuilder ChargedAttack) : ChargeAttackBuilder {
	public StringName ActionKey { get; private set; } = "attack_light";
	public ulong ChargeDuration { get; private set; } = 1000;
	public AttackBuilder UnchargedAttack { get; private set; } = UnchargedAttack;
	public AttackBuilder ChargedAttack { get; private set; } = ChargedAttack;



	public CompositeChargeAttackBuilder(
		AttackBuilder unchargedAttack,
		AttackBuilder chargedAttack,
		string actionKey = "attack_light",
		ulong chargeDuration = 1000
	) : this(unchargedAttack, chargedAttack) {
		(ActionKey, ChargeDuration) = (actionKey, chargeDuration);
	}



	public override CompositeChargeAttack Build(SingleWeapon weapon) {
		return new CompositeChargeAttack(weapon, this);
	}
}