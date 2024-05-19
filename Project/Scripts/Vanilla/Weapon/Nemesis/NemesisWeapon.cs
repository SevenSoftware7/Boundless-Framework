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


	public override void HandlePlayer(Player player) {
		base.HandlePlayer(player);

		if (! CanProcess()) return;

		if (player.Entity is null) return;

		if (player.InputDevice.IsActionJustPressed("attack_light")) {
			player.Entity.ExecuteAction(slashAttack with { });
		}
	}

	public override void _Ready() {
		base._Ready();

		slashAttack = new(this);
	}
}