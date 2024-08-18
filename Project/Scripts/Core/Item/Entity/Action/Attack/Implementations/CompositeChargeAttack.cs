namespace LandlessSkies.Core;

using System.Collections.Generic;
using Godot;
using SevenDev.Utility;

/// <summary>
/// A Charge Attack composed of two Attacks, with a determinate charge time.
/// </summary>
/// <param name="entity">Inherited from <see cref="Action"/>.</param>
/// <param name="weapon">Inherited from <see cref="Attack"/>.</param>
/// <param name="library">The Animation Library of the attacks</param>
/// <param name="info">The Composite Charge Attack Parameters to use when setting up the Charge Attack</param>
/// <param name="modifiers">Inherited from <see cref="Action"/>.</param>
public partial class CompositeChargeAttack(Entity entity, Weapon weapon, AnimationPath animationPath, CompositeChargeAttack.Builder info, IEnumerable<AttributeModifier>? modifiers = null) : ChargeAttack(entity, weapon, animationPath, modifiers) {
	private readonly TimeDuration chargeTime = new(true, info.ChargeDuration);
	private bool isDone;



	protected sealed override bool IsChargeStopped(InputDevice inputDevice) => !inputDevice.IsActionPressed(info.ActionInput);

	/// <summary>
	/// Called when the Attack finishes Charging.
	/// </summary>
	protected virtual void _ChargeDone() {
		GD.Print("Charge Done");
	}

	protected override void _Attack() {
		if (isDone) {
			Entity.ExecuteAction(new Wrapper(info.ChargedAttack, Weapon), true);
			_ChargedAttack();
			GD.Print("Full Charge Attack");
		}
		else {
			Entity.ExecuteAction(new Wrapper(info.UnchargedAttack, Weapon), true);
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
		if (!isDone && chargeTime.HasPassed) {
			isDone = true;
			_ChargeDone();
		}

		base.HandlePlayer(player);
	}



	public new sealed class Builder(Attack.Builder unchargedAttack, Attack.Builder chargedAttack, StringName AnimationName, StringName? actionKey = null, ulong? chargeDuration = null, IEnumerable<AttributeModifier>? modifiers = null) : ChargeAttack.Builder {
		public Attack.Builder UnchargedAttack { get; init; } = unchargedAttack;
		public Attack.Builder ChargedAttack { get; init; } = chargedAttack;


		public StringName ActionInput { get; init; } = actionKey ?? Inputs.AttackLight;
		public ulong ChargeDuration { get; init; } = chargeDuration ?? 1000;
		public AnimationPath AnimationPath = new();



		public void ExecuteOnKeyJustPressed(Player player, Weapon weapon) {
			if (player.InputDevice.IsActionJustPressed(ActionInput)) {
				player?.Entity?.ExecuteAction(new Wrapper(this, weapon));
			}
		}


		protected internal override CompositeChargeAttack Create(Entity entity, Weapon weapon) {
			return new CompositeChargeAttack(entity, weapon, new(weapon.LibraryName, AnimationName), this, modifiers);
		}
	}
}