namespace LandlessSkies.Vanilla;

using LandlessSkies.Core;
using System.Collections.Generic;
using Godot;

[Tool]
[GlobalClass]
public sealed partial class EleosWeapon : Weapon, IPlayerHandler {
	private readonly CompositeChargeAttackInfo chargeAttack = new(
		SlashAttackInfo.Instance, SlashAttackInfo.Instance,
		"Charge",
		Inputs.AttackHeavy, 750,
		[new PercentileModifier(Attributes.GenericMoveSpeed, -0.7f), new PercentileModifier(Attributes.GenericjumpHeight, -0.5f)]
	);

	public override WeaponType Type => WeaponType.Sword;
	public override WeaponUsage Usage => WeaponUsage.Slash | WeaponUsage.Thrust;
	public override WeaponSize Size => WeaponSize.OneHanded | WeaponSize.TwoHanded;


	private EleosWeapon() : this(null) { }
	public EleosWeapon(WeaponCostume? costume = null) : base(costume) { }


	public override Vector3 GetTipPosition() => new(0f, 5.3f, 0f);


	public override IEnumerable<AttackBuilder> GetAttacks(Entity target) {
		return [
			new AttackBuilder(SlashAttackInfo.Instance, this),
			new AttackBuilder(chargeAttack, this)
		];
	}

	public void HandlePlayer(Player player) {
		if (player.Entity is null) return;
		if (IsHolstered) return;

		switch (player.Entity.CurrentBehaviour) {
			case GroundedBehaviour grounded:
				if (player.InputDevice.IsActionJustPressed(Inputs.AttackLight)) {
					player.Entity.ExecuteAction(new AttackBuilder(SlashAttackInfo.Instance, this));
				}

				chargeAttack.ExecuteOnKeyJustPressed(player, this);
				break;
		}
	}
	public void DisavowPlayer() { }
}