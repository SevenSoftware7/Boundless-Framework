namespace LandlessSkies.Core;

using System.Collections.Generic;
using Godot;

public class CompositeChargeAttackInfo(
	AttackInfo unchargedAttack, AttackInfo chargedAttack,
	StringName? actionKey = null, ulong? chargeDuration = null, IEnumerable<AttributeModifier>? modifiers = null
) : ChargeAttackInfo {

	public AttackInfo UnchargedAttack { get; init; } = unchargedAttack;
	public AttackInfo ChargedAttack { get; init; } = chargedAttack;


	public StringName ActionInput { get; init; } = actionKey ?? Inputs.AttackLight;
	public ulong ChargeDuration { get; init; } = chargeDuration ?? 1000;



	public void ExecuteOnKeyJustPressed(Player player, Weapon weapon, StringName library) {
		if (player.InputDevice.IsActionJustPressed(ActionInput)) {
			player?.Entity?.ExecuteAction(new AttackBuilder(this, weapon, library));
		}
	}



	protected internal override CompositeChargeAttack Create(Entity entity, Weapon weapon, StringName library) {
		return new CompositeChargeAttack(entity, weapon, library, this, modifiers);
	}
}