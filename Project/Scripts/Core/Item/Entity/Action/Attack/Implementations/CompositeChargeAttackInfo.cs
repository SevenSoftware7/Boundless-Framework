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



	public void ExecuteOnKeyJustPressed(Player player, SingleWeapon weapon, StringName library) {
		if (player.InputDevice.IsActionJustPressed(ActionKey)) {
			player?.Entity?.ExecuteAction(new AttackBuilder(this, weapon, library));
		}
	}



	protected internal override CompositeChargeAttack Create(Entity entity, SingleWeapon weapon, StringName library) {
		return new CompositeChargeAttack(entity, weapon, library, this, Modifiers);
	}
}