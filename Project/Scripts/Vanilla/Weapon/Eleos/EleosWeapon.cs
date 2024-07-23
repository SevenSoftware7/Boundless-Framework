namespace LandlessSkies.Vanilla;

using LandlessSkies.Core;
using System.Collections.Generic;
using Godot;

[Tool]
[GlobalClass]
public sealed partial class EleosWeapon : Weapon, IPlayerHandler {

	private readonly CompositeChargeAttackInfo chargeAttack = new(
		SlashAttackInfo.Instance,
		SlashAttackInfo.Instance,
		Inputs.AttackHeavy,
		750,
		[new PercentileModifier(Attributes.GenericMoveSpeed, -0.7f), new PercentileModifier(Attributes.GenericjumpHeight, -0.5f)]
	);

	public override WeaponType Type => WeaponType.Sword;
	public override WeaponUsage Usage => WeaponUsage.Slash | WeaponUsage.Thrust;
	public override WeaponSize Size => WeaponSize.OneHanded | WeaponSize.TwoHanded;


	private EleosWeapon() : base() { }
	public EleosWeapon(WeaponCostume? costume = null) : base(costume) { }


	public override IEnumerable<AttackBuilder> GetAttacks(Entity target) {
		return [
			new AttackBuilder(SlashAttackInfo.Instance, this, LibraryName),
			new AttackBuilder(chargeAttack, this, LibraryName)
		];
	}

	public override void HandlePlayer(Player player) {
		base.HandlePlayer(player);
		if (IsHolstered) return;
		if (player.Entity is null) return;

		switch (player.Entity.CurrentBehaviour) {
			case GroundedBehaviour grounded:
				if (player.InputDevice.IsActionJustPressed(Inputs.AttackLight)) {
					player.Entity.ExecuteAction(new AttackBuilder(SlashAttackInfo.Instance, this, LibraryName));
				}

				chargeAttack.ExecuteOnKeyJustPressed(player, this, LibraryName);
				break;
		}
	}
	public override void DisavowPlayer() {
		base.DisavowPlayer();
	}
}