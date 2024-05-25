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



	public void ExecuteOnKeyJustPressed(Player player, SingleWeapon weapon) {
		if (player.InputDevice.IsActionJustPressed(ActionKey)) {
			player?.Entity?.ExecuteAction(GetInfo(weapon));
		}
	}


	public override CompositeChargeAttack Build(Entity entity, SingleWeapon weapon) {
		return new CompositeChargeAttack(entity, weapon, this, AttributeModifiers);
	}
}