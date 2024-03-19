namespace LandlessSkies.Core;

using System.Collections.Generic;
using Godot;

[Tool]
[GlobalClass]
public sealed partial class EpiphronWeapon : SingleWeapon<EpiphronWeaponData> {
	private EpiphronWeapon() : base() {}
	public EpiphronWeapon(EpiphronWeaponData? data = null, WeaponCostume? costume = null) : base(data, costume) {}



	public override IEnumerable<AttackAction.IInfo> GetAttacks(Entity target) {
		return [
			SlashAttack.DefaultInfo with { Weapon = this },
		];
	}


	public override void HandleInput(Player.InputInfo inputInfo) {
		base.HandleInput(inputInfo);

		if (Entity is null)
			return;

		if (inputInfo.InputDevice.IsActionJustPressed("attack_light")) {
			Entity.ExecuteAction(SlashAttack.DefaultInfo with { Weapon = this });
		}
	}
}