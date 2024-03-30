using Godot;

namespace LandlessSkies.Core;

public partial class CompositeChargeAttack(SingleWeapon weapon, CompositeChargeAttackInfo info) : ChargeAttack(weapon) {
	public override ulong ChargeDuration => info.ChargeDuration;



	protected override bool IsChargeStopped(InputDevice inputDevice) {
		return inputDevice.IsActionJustReleased(info.ActionKey);
	}


	protected override void ChargeDone() {}

	protected override void ChargedAttack() {
		Weapon.Entity?.ExecuteAction(info.ChargedAttack with {}, true);
	}

	protected override void UnchargedAttack() {
		Weapon.Entity?.ExecuteAction(info.UnchargedAttack with {}, true);
	}
}