using Godot;

namespace LandlessSkies.Core;

public partial class CompositeChargeAttack(SingleWeapon weapon, CompositeChargeAttackBuilder info) : ChargeAttack(weapon, info.ChargeDuration) {


	protected override bool IsChargeStopped(InputDevice inputDevice) {
		return inputDevice.IsActionJustReleased(info.ActionKey);
	}


	protected override void ChargeDone(Entity entity) {
		GD.Print("Charged Up");
	}

	protected override void ChargedAttack(Entity entity) {
		QueueFree();
		entity.ExecuteAction(new AttackActionInfo(Weapon, info.ChargedAttack), true);
	}

	protected override void UnchargedAttack(Entity entity) {
		QueueFree();
		entity.ExecuteAction(new AttackActionInfo(Weapon, info.UnchargedAttack), true);
	}
}