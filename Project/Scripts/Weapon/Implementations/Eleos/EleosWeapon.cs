using System;
using System.Collections.Generic;
using Godot;
using SevenGame.Utility;

namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class EleosWeapon : SingleWeapon {
	public SlashAttack.Info SlashAttack { get; init; }



	protected EleosWeapon() : base() {
		SlashAttack = new(this);
	}
	public EleosWeapon(EleosWeaponData data, WeaponCostume? costume) : base(data, costume) {
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
		if ( Entity.CurrentAction is EntityAction action && action.IsCancellable ) return;

		if ( inputInfo.ControlDevice.IsInputJustPressed(ControlDevice.InputType.LightAttack) ) {
			Entity.ExecuteAction(SlashAttack);
		}
	}

    public override void _Ready() {
		if (Data is null) {
			SetData(ResourceManager.GetRegisteredWeapon<EleosWeaponData>());
		}
        base._Ready();
    }
}