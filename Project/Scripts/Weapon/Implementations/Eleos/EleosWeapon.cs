namespace LandlessSkies.Core;

using System.Collections.Generic;
using Godot;

[Tool]
[GlobalClass]
public partial class EleosWeapon : SingleWeapon {
	protected EleosWeapon() : base() {}
	public EleosWeapon(EleosWeaponData? data = null, WeaponCostume? costume = null) : base(data ?? ResourceManager.GetRegisteredWeapon<EleosWeaponData>(), costume) {}



	public override IEnumerable<AttackAction.IInfo> GetAttacks(Entity target) {
		return [
			SlashAttack.DefaultInfo with { Weapon = this },
		];
	}


	public override void HandleInput(Player.InputInfo inputInfo) {
		base.HandleInput(inputInfo);

		if (Entity is null)
			return;

		if (inputInfo.ControlDevice.IsInputJustPressed(ControlDevice.InputType.LightAttack)) {
			Entity.ExecuteAction(SlashAttack.DefaultInfo with { Weapon = this });
		}
	}

	public override void _Ready() {
		if (Data is null) {
			SetData(ResourceManager.GetRegisteredWeapon<EleosWeaponData>());
		}
		base._Ready();
	}
}