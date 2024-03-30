namespace LandlessSkies.Core;

using System.Collections.Generic;
using Godot;

[Tool]
[GlobalClass]
public sealed partial class NemesisWeapon(NemesisWeaponData? data = null, WeaponCostume? costume = null) : SingleWeapon<NemesisWeaponData>(data, costume) {
	private SlashAttackInfo slashAttack = null!;


	private NemesisWeapon() : this(null) {}


	public override IEnumerable<AttackInfo> GetAttacks(Entity target) {
		return [
			slashAttack,
		];
	}


	public override void HandleInput(CameraController3D cameraController, InputDevice inputDevice) {
		base.HandleInput(cameraController, inputDevice);

		if (! CanProcess()) {
			return;
		}

		if (Entity is null)
			return;

		if (inputDevice.IsActionJustPressed("attack_light")) {
			Entity.ExecuteAction(slashAttack with {});
		}
	}

	public override void _Ready() {
		base._Ready();

		slashAttack = new(this);
	}
}