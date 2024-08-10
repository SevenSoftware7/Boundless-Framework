namespace LandlessSkies.Vanilla;

using LandlessSkies.Core;
using System.Collections.Generic;
using Godot;

[Tool]
[GlobalClass]
public sealed partial class NemesisWeapon : Weapon, IPlayerHandler {
	public override WeaponType Type => WeaponType.Sword;
	public override WeaponUsage Usage => WeaponUsage.Slash | WeaponUsage.Strike;
	public override WeaponSize Size => WeaponSize.TwoHanded;


	private NemesisWeapon() : base() { }
	public NemesisWeapon(WeaponCostume? costume = null) : base(costume) { }


	public override Vector3 GetTipPosition() => new(0f, 3.2f, 0f);

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