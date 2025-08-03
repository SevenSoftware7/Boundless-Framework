namespace EndlessTwilight;

using System.Collections.Generic;
using Godot;
using Seven.Boundless;

[Tool]
[GlobalClass]
public sealed partial class PaxWeapon : Weapon, IPlayerHandler {
	private readonly SlashAttack.Builder slashAttack;
	public override WeaponKind Kind { get; } = WeaponKind.Sword;
	public override WeaponUsage Usage { get; } = WeaponUsage.Slash | WeaponUsage.Strike;
	public override WeaponSize Size { get; } = WeaponSize.TwoHanded;

	public override float Length { get; } = 3.2f;


	public PaxWeapon() : base() {
		slashAttack = new(this);
	}


	public override IEnumerable<EntityAction.Wrapper> GetAttacks(Entity target) {
		return [
			slashAttack
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