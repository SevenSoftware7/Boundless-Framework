using System;
using System.Collections.Generic;
using Godot;

namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class EleosWeapon : SimpleWeapon {
	private Attacks attacks;



	protected EleosWeapon() : base() {}
	public EleosWeapon(EleosWeaponData data, WeaponCostume? costume, Node3D root) : base(data, costume, root) {}



	protected override void InitializeAttacks() {
		base.InitializeAttacks();
		attacks = new(this);
	}

	public override IEnumerable<AttackAction.IAttackInfo> GetAttacks(Entity target) {
		return [
			attacks.slashAttack,
		];
	}


	public override void HandleInput(Player.InputInfo inputInfo) {
		base.HandleInput(inputInfo);

		if ( Entity is null ) return;
		if ( Entity.CurrentAction is EntityAction action && action.IsCancellable ) return;

		if ( inputInfo.ControlDevice.IsInputJustPressed(ControlDevice.InputType.LightAttack) ) {
			Entity.ExecuteAction(attacks.slashAttack);
		}
	}



	private readonly struct Attacks(EleosWeapon weapon) {
		public readonly SlashAttack.Info slashAttack = new(weapon);

	}
}