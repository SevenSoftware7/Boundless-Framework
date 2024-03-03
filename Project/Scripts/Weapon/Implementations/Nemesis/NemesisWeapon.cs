using System;
using System.Collections.Generic;
using Godot;
using SevenGame.Utility;

namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class NemesisWeapon : SingleWeapon {
	public SlashAttack.Info SlashAttack { get; init; }



	protected NemesisWeapon() : base() {
		SlashAttack = new(this);
	}
	public NemesisWeapon(NemesisWeaponData data, WeaponCostume? costume) : base(data, costume) {
		SlashAttack = new(this);
	}



	public override IEnumerable<AttackAction.IInfo> GetAttacks(Entity target) {
		return [
			SlashAttack,
		];
	}


	public override void HandleInput(Player.InputInfo inputInfo) {
		base.HandleInput(inputInfo);

		if ( Entity is null ) return;

		if ( inputInfo.ControlDevice.IsInputJustPressed(ControlDevice.InputType.LightAttack) ) {
			Entity.ExecuteAction(SlashAttack);
		}
	}

    public override void _Ready() {
		if (Data is null) {
			SetData(ResourceManager.GetRegisteredWeapon<NemesisWeaponData>());
		}
        base._Ready();
    }
}