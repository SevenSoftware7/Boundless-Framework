namespace LandlessSkies.Vanilla;

using LandlessSkies.Core;
using System.Collections.Generic;
using Godot;

[Tool]
[GlobalClass]
public sealed partial class EpiphronWeapon : SingleWeapon {
	public override IWeapon.Type WeaponType => IWeapon.Type.Sword;
	public override IWeapon.Usage WeaponUsage => IWeapon.Usage.Slash | IWeapon.Usage.Thrust;
	public override IWeapon.Size WeaponSize => IWeapon.Size.OneHanded;


	private EpiphronWeapon() : base() { }
	public EpiphronWeapon(WeaponCostume? costume = null) : base(costume) { }


	public override IEnumerable<AttackActionInfo> GetAttacks(Entity target) {
		return [
			SlashAttackBuilder.Instance.GetInfo(this),
		];
	}


	public override void HandlePlayer(Player player) {
		base.HandlePlayer(player);

		if (!CanProcess()) return;

		if (player.Entity is null) return;

		if (player.InputDevice.IsActionJustPressed("attack_light")) {
			player.Entity.ExecuteAction(SlashAttackBuilder.Instance.GetInfo(this));
		}
	}
}