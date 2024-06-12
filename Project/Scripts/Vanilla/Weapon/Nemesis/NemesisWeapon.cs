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


	public override IEnumerable<AttackBuilder> GetAttacks(Entity target) {
		return [
			new AttackBuilder(SlashAttackInfo.Instance, this),
		];
	}


	public void HandlePlayer(Player player) {
		if (player.Entity is null) return;

		switch (player.Entity.CurrentBehaviour) {
		case GroundedBehaviour grounded:
			if (player.InputDevice.IsActionJustPressed(Inputs.AttackLight)) {
				player.Entity.ExecuteAction(new AttackBuilder(SlashAttackInfo.Instance, this));
			}
			break;
		}
	}
	public void DisavowPlayer() { }
}