namespace LandlessSkies.Vanilla;

using LandlessSkies.Core;
using System.Collections.Generic;
using Godot;

[Tool]
[GlobalClass]
public sealed partial class NemesisWeapon : SingleWeapon {
	private SlashAttackInfo slashAttack = null!;

	public override IWeapon.Type WeaponType => IWeapon.Type.Sword;
	public override IWeapon.Usage WeaponUsage => IWeapon.Usage.Slash | IWeapon.Usage.Strike;
	public override IWeapon.Size WeaponSize => IWeapon.Size.TwoHanded;


	private NemesisWeapon() : base() { }
	public NemesisWeapon(WeaponCostume? costume = null) : base(costume) { }


	public override IEnumerable<AttackInfo> GetAttacks(Entity target) {
		return [
			slashAttack,
		];
	}


	public override void HandleInput(Entity entity, CameraController3D cameraController, InputDevice inputDevice) {
		base.HandleInput(entity, cameraController, inputDevice);

		if (!CanProcess()) {
			return;
		}

		if (entity is null)
			return;

		if (inputDevice.IsActionJustPressed("attack_light")) {
			entity.ExecuteAction(slashAttack with { });
		}
	}

	public override void _Ready() {
		base._Ready();

		slashAttack = new(this);
	}
}