namespace LandlessSkies.Vanilla;

using System.Collections.Generic;
using Godot;
using LandlessSkies.Core;

[Tool]
[GlobalClass]
public sealed partial class EleosWeapon : Weapon, IPlayerHandler {
	private readonly SlashAttack.Builder slashAttack;
	private readonly CompositeChargeAttack.Builder chargeAttack;

	public override IWeapon.WeaponKind Kind => IWeapon.WeaponKind.Sword;
	public override IWeapon.WeaponUsage Usage => IWeapon.WeaponUsage.Slash | IWeapon.WeaponUsage.Thrust;
	public override IWeapon.WeaponSize Size => IWeapon.WeaponSize.OneHanded | IWeapon.WeaponSize.TwoHanded;


	public EleosWeapon() : base() {
		slashAttack = new(this);
		chargeAttack = new(
			this,
			unchargedAttack: slashAttack, chargedAttack: slashAttack,
			AnimationName: "Charge", actionKey: Inputs.AttackHeavy, chargeDuration: 750,
			[new PercentileModifier(Traits.GenericMoveSpeed, -0.7f), new PercentileModifier(Traits.GenericJumpHeight, -0.5f)]
		);
	}


	public override Vector3 GetTipPosition() => new(0f, 5.3f, 0f);


	public override IEnumerable<Action.Wrapper> GetAttacks(Entity target) {
		return [
			slashAttack,
			chargeAttack
		];
	}

	public void HandlePlayer(Player player) {
		if (player.Entity is null) return;
		if (HolsterState) return;

		switch (player.Entity.CurrentBehaviour) {
			case GroundedBehaviour grounded:
				if (player.InputDevice.IsActionJustPressed(Inputs.AttackLight)) {
					player.Entity.ExecuteAction(slashAttack);
				}

				chargeAttack.ExecuteOnKeyJustPressed(player);
				break;
		}
	}
	public void DisavowPlayer() { }
}