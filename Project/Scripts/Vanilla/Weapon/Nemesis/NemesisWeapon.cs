namespace LandlessSkies.Vanilla;

using LandlessSkies.Core;
using System.Collections.Generic;
using Godot;

[Tool]
[GlobalClass]
public sealed partial class NemesisWeapon : SingleWeapon, IPlayerHandler {
	public override IWeapon.Type WeaponType => IWeapon.Type.Sword;
	public override IWeapon.Usage WeaponUsage => IWeapon.Usage.Slash | IWeapon.Usage.Strike;
	public override IWeapon.Size WeaponSize => IWeapon.Size.TwoHanded;


	private NemesisWeapon() : base() { }
	public NemesisWeapon(WeaponCostume? costume = null) : base(costume) { }


	public override IEnumerable<AttackActionInfo> GetAttacks(Entity target) {
		return [
			SlashAttackBuilder.Instance.GetInfo(this),
		];
	}


	public void SetupPlayer(Player player) { }
	public void HandlePlayer(Player player) {
		if (player.Entity is null) return;

		switch (player.Entity.CurrentBehaviour) {
		case GroundedBehaviour grounded:
			if (player.InputDevice.IsActionJustPressed(Inputs.AttackLight)) {
				player.Entity.ExecuteAction(SlashAttackBuilder.Instance.GetInfo(this));
			}
			break;
		}
	}
	public void DisavowPlayer() { }
}