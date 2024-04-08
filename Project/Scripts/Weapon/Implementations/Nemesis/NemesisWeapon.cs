namespace LandlessSkies.Core;

using System.Collections.Generic;
using Godot;

[Tool]
[GlobalClass]
public sealed partial class NemesisWeapon : SingleWeapon<NemesisWeaponData> {
	private SlashAttackInfo slashAttack = null!;


	public NemesisWeapon(NemesisWeaponData? data = null, WeaponCostume? costume = null) : base(data, costume) {}
	private NemesisWeapon() : base() {}


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