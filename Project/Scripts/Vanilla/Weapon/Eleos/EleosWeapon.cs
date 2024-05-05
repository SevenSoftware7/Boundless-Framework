namespace LandlessSkies.Vanilla;

using LandlessSkies.Core;
using System.Collections.Generic;
using Godot;

[Tool]
[GlobalClass]
public sealed partial class EleosWeapon : SingleWeapon {
	private SlashAttackInfo slashAttack = null!;
	private CompositeChargeAttackInfo chargeAttack = null!;

	public override IWeapon.Type WeaponType => IWeapon.Type.Sword;
	public override IWeapon.Usage WeaponUsage => IWeapon.Usage.Slash | IWeapon.Usage.Thrust;
	public override IWeapon.Size WeaponSize => IWeapon.Size.OneHanded |IWeapon.Size.TwoHanded;


	public EleosWeapon(WeaponCostume? costume = null) : base(costume) { }
	private EleosWeapon() : base() { }

	public override IEnumerable<AttackInfo> GetAttacks(Entity target) {
		return [
			slashAttack,
			chargeAttack
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

		if (inputDevice.IsActionJustPressed("attack_heavy")) {
			entity.ExecuteAction(chargeAttack);
		}
	}


	public override void _Ready() {
		base._Ready();

		slashAttack = new(this);
		chargeAttack = new(this, slashAttack, slashAttack, "attack_heavy");
	}
}