namespace LandlessSkies.Core;

using System.Collections.Generic;
using Godot;

public class CompositeChargeAttackInfo(
	AttackInfo unchargedAttack, AttackInfo chargedAttack,
	StringName? actionKey = null, ulong? chargeDuration = null, IEnumerable<AttributeModifier>? modifiers = null
) : ChargeAttackInfo {

	public AttackInfo UnchargedAttack { get; init; } = unchargedAttack;
	public AttackInfo ChargedAttack { get; init; } = chargedAttack;


	public StringName ActionKey { get; init; } = actionKey ?? Inputs.AttackLight;
	public ulong ChargeDuration { get; init; } = chargeDuration ?? 1000;
	public IEnumerable<AttributeModifier> Modifiers { get; init; } = modifiers ?? [];



	public void ExecuteOnKeyJustPressed(Player player, SingleWeapon weapon) {
		if (player.InputDevice.IsActionJustPressed(ActionKey)) {
			player?.Entity?.ExecuteAction(new AttackBuilder(this, weapon));
		}
	}



	protected internal override CompositeChargeAttack Create(Entity entity, SingleWeapon weapon) {
		return new CompositeChargeAttack(entity, weapon, this, Modifiers);
	}
}