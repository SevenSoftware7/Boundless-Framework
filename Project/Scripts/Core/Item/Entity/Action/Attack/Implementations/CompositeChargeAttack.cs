using System.Collections.Generic;
using Godot;
using SevenDev.Utility;

namespace LandlessSkies.Core;

/// <summary>
/// A Charge Attack composed of two Attacks, with a determinate charge time.
/// </summary>
/// <param name="entity">Inherited from <see cref="EntityAction"/>.</param>
/// <param name="weapon">Inherited from <see cref="Attack"/>.</param>
/// <param name="library">The Animation Library of the attacks</param>
/// <param name="info">The Composite Charge Attack Parameters to use when setting up the Charge Attack</param>
/// <param name="modifiers">Inherited from <see cref="EntityAction"/>.</param>
public partial class CompositeChargeAttack(Entity entity, Weapon weapon, StringName library, CompositeChargeAttackInfo info, IEnumerable<AttributeModifier>? modifiers = null) : ChargeAttack(entity, weapon, modifiers) {
	private readonly TimeDuration chargeTime = new(info.ChargeDuration);
	private bool isDone;


	protected sealed override bool IsChargeStopped(InputDevice inputDevice) => ! inputDevice.IsActionPressed(info.ActionKey);

	/// <summary>
	/// Called when the Attack finishes Charging.
	/// </summary>
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

	/// <summary>
	/// Callback Method called when the Attack is launched after being fully charged.
	/// </summary>
	protected virtual void _ChargedAttack() { }

	/// <summary>
	/// Callback Method called when the Attack is launched prematurely, usually a weaker version of <see cref="_ChargedAttack"/>.
	/// </summary>
	protected virtual void _UnchargedAttack() { }

	public override void HandlePlayer(Player player) {
		if (! isDone && chargeTime.IsDone) {
			isDone = true;
			_ChargeDone();
		}

		base.HandlePlayer(player);
	}
}