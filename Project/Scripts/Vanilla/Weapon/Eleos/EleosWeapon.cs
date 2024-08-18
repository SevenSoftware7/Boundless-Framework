namespace LandlessSkies.Vanilla;

using LandlessSkies.Core;
using System.Collections.Generic;
using Godot;

[Tool]
[GlobalClass]
public sealed partial class EleosWeapon : Weapon, IPlayerHandler {
	private readonly SlashAttack.Builder slashAttack;
	private readonly CompositeChargeAttack.Builder chargeAttack;

	public override WeaponType Type => WeaponType.Sword;
	public override WeaponUsage Usage => WeaponUsage.Slash | WeaponUsage.Thrust;
	public override WeaponSize Size => WeaponSize.OneHanded | WeaponSize.TwoHanded;


	private EleosWeapon() : this(null) { }
	public EleosWeapon(WeaponCostume? costume = null) : base(costume) {
		slashAttack = new(this);
		chargeAttack = new(
			this,
			slashAttack, slashAttack,
			"Charge",
			Inputs.AttackHeavy, 750,
			[new PercentileModifier(Attributes.GenericMoveSpeed, -0.7f), new PercentileModifier(Attributes.GenericjumpHeight, -0.5f)]
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
		if (IsHolstered) return;

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