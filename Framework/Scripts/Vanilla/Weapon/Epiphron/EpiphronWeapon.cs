namespace EndlessTwilight;

using System.Collections.Generic;
using Godot;
using SevenDev.Boundless;

[Tool]
[GlobalClass]
public sealed partial class EpiphronWeapon : Weapon, IPlayerHandler {
	private readonly SlashAttack.Builder slashAttack;
	public override IWeapon.WeaponKind Kind => IWeapon.WeaponKind.Sword;
	public override IWeapon.WeaponUsage Usage => IWeapon.WeaponUsage.Slash | IWeapon.WeaponUsage.Thrust;
	public override IWeapon.WeaponSize Size => IWeapon.WeaponSize.OneHanded;


	public EpiphronWeapon() : base() {
		slashAttack = new(this);
	}


	public override Transform3D GetTipTransform() => GlobalTransform.TranslatedLocal(new(0f, 1.6f, 0f));

	public override IEnumerable<EntityAction.Wrapper> GetAttacks(Entity target) {
		return [
			slashAttack,
		];
	}


	public void HandlePlayer(Player player) {
		if (player.Entity is null) return;
		if (HolsterState) return;

		switch (player.Entity.CurrentBehaviour) {
			case GroundedBehaviour grounded:
				if (player.InputDevice.IsActionJustPressed(Inputs.AttackLight)) {
					player.Entity.ExecuteAction(slashAttack);
				}
				break;
		}
	}
	public void DisavowPlayer() { }
}