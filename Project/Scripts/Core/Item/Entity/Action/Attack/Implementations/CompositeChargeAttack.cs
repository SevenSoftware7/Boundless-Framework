using System.Collections.Generic;
using Godot;
using KGySoft.CoreLibraries;
using SevenDev.Utility;

namespace LandlessSkies.Core;

public partial class CompositeChargeAttack(Entity entity, Weapon weapon, StringName library, CompositeChargeAttackInfo info, IEnumerable<AttributeModifier> modifiers) : ChargeAttack(entity, weapon, info.ChargeDuration) {


	protected override bool IsChargeStopped(InputDevice inputDevice) {
		return inputDevice.IsActionJustReleased(info.ActionKey);
	}


	protected override void ChargeDone() {
		GD.Print("Charged Up");
	}

	protected override void ChargedAttack() {
		QueueFree();
		Entity.ExecuteAction(new AttackBuilder(info.ChargedAttack, Weapon, library), true);
	}

	protected override void UnchargedAttack() {
		QueueFree();
		Entity.ExecuteAction(new AttackBuilder(info.UnchargedAttack, Weapon, library), true);
	}


	protected override void _Start() {
		Entity.AttributeModifiers.AddRange(modifiers);
	}

	protected override void _Stop() {
		Entity.AttributeModifiers.RemoveRange(modifiers);
	}
}