namespace LandlessSkies.Vanilla;

using LandlessSkies.Core;
using System.Collections.Generic;
using Godot;

[Tool]
[GlobalClass]
public sealed partial class EleosWeapon : SingleWeapon, IPlayerHandler {
	private readonly CompositeChargeAttackInfo chargeAttack = new(
		SlashAttackInfo.Instance,
		SlashAttackInfo.Instance,
		Inputs.AttackHeavy,
		750,
		[new PercentileModifier(Attributes.GenericMoveSpeed, -0.7f), new PercentileModifier(Attributes.GenericjumpHeight, -0.5f)]
	);

	public override IWeapon.Type WeaponType => IWeapon.Type.Sword;
	public override IWeapon.Usage WeaponUsage => IWeapon.Usage.Slash | IWeapon.Usage.Thrust;
	public override IWeapon.Size WeaponSize => IWeapon.Size.OneHanded | IWeapon.Size.TwoHanded;


	private EleosWeapon() : base() { }
	public EleosWeapon(WeaponCostume? costume = null) : base(costume) { }


	public override IEnumerable<AttackBuilder> GetAttacks(Entity target) {
		return [
			new AttackBuilder(SlashAttackInfo.Instance, this),
			new AttackBuilder(chargeAttack, this)
		];
	}

	public void HandlePlayer(Player player) {
		if (player.Entity is null) return;

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