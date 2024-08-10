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


	public override Vector3 GetTipPosition() => new(0f, 1.6f, 0f);

	public override IEnumerable<AttackBuilder> GetAttacks(Entity target) {
		return [
			new AttackBuilder(SlashAttackInfo.Instance, this),
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
				break;
		}
	}
	public void DisavowPlayer() { }

}