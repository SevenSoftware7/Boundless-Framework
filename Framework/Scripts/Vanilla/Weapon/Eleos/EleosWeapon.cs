namespace EndlessTwilight;

using System.Collections.Generic;
using Godot;
using Seven.Boundless;

[Tool]
[GlobalClass]
public sealed partial class EleosWeapon : Weapon, IPlayerHandler {
	private readonly SlashAttack.Builder slashAttack;
	private readonly CompositeChargeAttack.Builder chargeAttack;

	public override WeaponKind Kind { get; } = WeaponKind.Sword;
	public override WeaponUsage Usage { get; } = WeaponUsage.Slash | WeaponUsage.Thrust;
	public override WeaponSize Size { get; } = WeaponSize.OneHanded | WeaponSize.TwoHanded;

	public override float Length { get; } = 5.3f;


	public EleosWeapon() : base() {
		slashAttack = new(this);
		chargeAttack = new(
			this,
			unchargedAttack: slashAttack, chargedAttack: slashAttack,
			AnimationName: "Charge", actionKey: Inputs.AttackHeavy, chargeDuration: 750,
			[new PercentileModifier(Traits.GenericMoveSpeed, -0.7f), new PercentileModifier(Traits.GenericJumpHeight, -0.5f)]
		);
	}


	public override IEnumerable<EntityAction.Wrapper> GetAttacks(Entity target) {
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