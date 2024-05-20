namespace LandlessSkies.Core;

public partial class CompositeChargeAttack(SingleWeapon weapon, CompositeChargeAttackInfo info) : ChargeAttack(weapon) {
	public override ulong ChargeDuration => info.ChargeDuration;



	protected override bool IsChargeStopped(InputDevice inputDevice) {
		return inputDevice.IsActionJustReleased(info.ActionKey);
	}


	protected override void ChargeDone(Entity entity) { }

	protected override void ChargedAttack(Entity entity) {
		entity.ExecuteAction(info.ChargedAttack with { }, true);
	}

	protected override void UnchargedAttack(Entity entity) {
		entity.ExecuteAction(info.UnchargedAttack with { }, true);
	}
}