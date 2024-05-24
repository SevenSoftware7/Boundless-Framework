namespace LandlessSkies.Core;

using System.Collections.Generic;
using Godot;

public class CompositeChargeAttackBuilder(AttackBuilder UnchargedAttack, AttackBuilder ChargedAttack) : ChargeAttackBuilder {
	public StringName ActionKey { get; private set; } = "attack_light";
	public ulong ChargeDuration { get; private set; } = 1000;
	public IEnumerable<AttributeModifier> AttributeModifiers { get; private set; } = [];
	public AttackBuilder UnchargedAttack { get; private set; } = UnchargedAttack;
	public AttackBuilder ChargedAttack { get; private set; } = ChargedAttack;



	public CompositeChargeAttackBuilder(
		AttackBuilder unchargedAttack,
		AttackBuilder chargedAttack,
		string actionKey = "attack_light",
		ulong chargeDuration = 1000,
		IEnumerable<AttributeModifier> modifiers = null!
	) : this(unchargedAttack, chargedAttack) {
		(ActionKey, ChargeDuration, AttributeModifiers) = (actionKey, chargeDuration, modifiers ?? []);
	}



	public override CompositeChargeAttack Build(SingleWeapon weapon) {
		return new CompositeChargeAttack(weapon, this, AttributeModifiers);
	}
}