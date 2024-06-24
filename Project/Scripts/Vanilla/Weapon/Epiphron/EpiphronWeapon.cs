namespace LandlessSkies.Vanilla;

using LandlessSkies.Core;
using System.Collections.Generic;
using Godot;

[Tool]
[GlobalClass]
public sealed partial class EpiphronWeapon : Weapon, IPlayerHandler {
	public override WeaponType Type => WeaponType.Sword;
	public override WeaponUsage Usage => WeaponUsage.Slash | WeaponUsage.Thrust;
	public override WeaponSize Size => WeaponSize.OneHanded;


	private EpiphronWeapon() : base() { }
	public EpiphronWeapon(WeaponCostume? costume = null) : base(costume) { }


	public override IEnumerable<AttackBuilder> GetAttacks(Entity target) {
		return [
			new AttackBuilder(SlashAttackInfo.Instance, this, LibraryName),
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
			break;
		}
	}
	public override void DisavowPlayer() {
		base.DisavowPlayer();
	}
}