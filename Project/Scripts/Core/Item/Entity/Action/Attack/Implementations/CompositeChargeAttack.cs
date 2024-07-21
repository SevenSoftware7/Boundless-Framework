using System.Collections.Generic;
using Godot;
using SevenDev.Utility;

namespace LandlessSkies.Core;

public partial class CompositeChargeAttack(Entity entity, Weapon weapon, StringName library, CompositeChargeAttackInfo info, IEnumerable<AttributeModifier>? modifiers = null) : ChargeAttack(entity, weapon, modifiers) {
	private readonly TimeDuration chargeTime = new(info.ChargeDuration);
	private bool isDone;


	protected sealed override bool IsChargeStopped(InputDevice inputDevice) => ! inputDevice.IsActionPressed(info.ActionKey);


	protected virtual void _ChargeDone() {
		GD.Print("Charge Done");
	}

	protected override void _Attack() {
		if (isDone) {
			Entity.ExecuteAction(new AttackBuilder(info.ChargedAttack, Weapon, library), true);
			_ChargedAttack();
			GD.Print("Full Charge Attack");
		}
		else {
			Entity.ExecuteAction(new AttackBuilder(info.UnchargedAttack, Weapon, library), true);
			_UnchargedAttack();
			GD.Print("Premature Charge Attack");
		}
	}
	protected virtual void _ChargedAttack() { }
	protected virtual void _UnchargedAttack() { }

	public override void HandlePlayer(Player player) {
		if (! isDone && chargeTime.IsDone) {
			isDone = true;
			_ChargeDone();
		}

		base.HandlePlayer(player);
	}
}