namespace LandlessSkies.Core;

using System.Collections.Generic;
using Godot;
using SevenDev.Utility;


public class CompositeChargeAttackInfo(
	AttackInfo unchargedAttack, AttackInfo chargedAttack,
	StringName AnimationName,
	StringName? actionKey = null, ulong? chargeDuration = null, IEnumerable<AttributeModifier>? modifiers = null
) : ChargeAttackInfo {

	public AttackInfo UnchargedAttack { get; init; } = unchargedAttack;
	public AttackInfo ChargedAttack { get; init; } = chargedAttack;


	public StringName ActionInput { get; init; } = actionKey ?? Inputs.AttackLight;
	public ulong ChargeDuration { get; init; } = chargeDuration ?? 1000;
	public AnimationPath AnimationPath = new();



	public void ExecuteOnKeyJustPressed(Player player, Weapon weapon) {
		if (player.InputDevice.IsActionJustPressed(ActionInput)) {
			player?.Entity?.ExecuteAction(new AttackBuilder(this, weapon));
		}
	}



	protected internal override CompositeChargeAttack Create(Entity entity, Weapon weapon) {
		return new CompositeChargeAttack(entity, weapon, new(weapon.LibraryName, AnimationName), this, modifiers);
	}
}