namespace LandlessSkies.Vanilla;

using LandlessSkies.Core;
using System.Collections.Generic;
using Godot;

[Tool]
[GlobalClass]
public sealed partial class EleosWeapon : SingleWeapon, IPlayerHandler {
	private readonly CompositeChargeAttackBuilder chargeAttack = new(
		SlashAttackBuilder.Instance,
		SlashAttackBuilder.Instance,
		"attack_heavy",
		750,
		[new PercentileModifier(Attributes.GenericMoveSpeed, -0.7f), new PercentileModifier(Attributes.GenericjumpHeight, -0.5f)]
	);

	public override IWeapon.Type WeaponType => IWeapon.Type.Sword;
	public override IWeapon.Usage WeaponUsage => IWeapon.Usage.Slash | IWeapon.Usage.Thrust;
	public override IWeapon.Size WeaponSize => IWeapon.Size.OneHanded | IWeapon.Size.TwoHanded;


	public EleosWeapon(WeaponCostume? costume = null) : base(costume) { }
	private EleosWeapon() : base() { }

	public override IEnumerable<AttackActionInfo> GetAttacks(Entity target) {
		return [
			SlashAttackBuilder.Instance.GetInfo(this),
			chargeAttack.GetInfo(this)
		];
	}

	public void SetupPlayer(Player player) { }
	public void HandlePlayer(Player player) {
		if (!CanProcess()) return;

		if (player.Entity is null) return;

		if (player.InputDevice.IsActionJustPressed("attack_light")) {
			player.Entity.ExecuteAction(SlashAttackBuilder.Instance.GetInfo(this));
		}

		chargeAttack.ExecuteOnKeyJustPressed(player, this);
	}
	public void DisavowPlayer(Player player) { }
}