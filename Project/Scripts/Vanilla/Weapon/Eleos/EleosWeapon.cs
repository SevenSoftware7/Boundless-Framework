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


	public override void HandlePlayer(Player player) {
		base.HandlePlayer(player);

		if (!CanProcess()) return;

		if (player.Entity is null) return;

		if (player.InputDevice.IsActionJustPressed("attack_light")) {
			player.Entity.ExecuteAction(slashAttack with { });
		}

		if (player.InputDevice.IsActionJustPressed("attack_heavy")) {
			player.Entity.ExecuteAction(chargeAttack);
		}
	}


	public override void _Ready() {
		base._Ready();

		slashAttack = new(this);
		chargeAttack = new(this, slashAttack, slashAttack, "attack_heavy");
	}
}