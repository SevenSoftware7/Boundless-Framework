namespace LandlessSkies.Core;

using System.Collections.Generic;
using Godot;

public class CompositeChargeAttackBuilder(
	AttackBuilder UnchargedAttack, AttackBuilder ChargedAttack,
	StringName? actionKey = null, ulong? chargeDuration = null, IEnumerable<AttributeModifier>? attributes = null
) : ChargeAttackBuilder {

	public AttackBuilder UnchargedAttack { get; init; } = UnchargedAttack;
	public AttackBuilder ChargedAttack { get; init; } = ChargedAttack;


	public StringName ActionKey { get; init; } = actionKey ?? Inputs.AttackLight;
	public ulong ChargeDuration { get; init; } = chargeDuration ?? 1000;
	public IEnumerable<AttributeModifier> AttributeModifiers { get; init; } = attributes ?? [];



	public void ExecuteOnKeyJustPressed(Player player, SingleWeapon weapon) {
		if (player.InputDevice.IsActionJustPressed(ActionKey)) {
			player?.Entity?.ExecuteAction(GetInfo(weapon));
		}
	}


	public override CompositeChargeAttack Build(Entity entity, SingleWeapon weapon) {
		return new CompositeChargeAttack(entity, weapon, this, AttributeModifiers);
	}
}