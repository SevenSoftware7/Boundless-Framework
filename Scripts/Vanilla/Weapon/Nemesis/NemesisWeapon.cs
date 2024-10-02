namespace LandlessSkies.Vanilla;

using LandlessSkies.Core;
using System.Collections.Generic;
using Godot;

[Tool]
[GlobalClass]
public sealed partial class NemesisWeapon : Weapon, IPlayerHandler {
	private readonly SlashAttack.Builder slashAttack;
	public override WeaponType Type => WeaponType.Sword;
	public override WeaponUsage Usage => WeaponUsage.Slash | WeaponUsage.Strike;
	public override WeaponSize Size => WeaponSize.TwoHanded;


	private NemesisWeapon() : this(null) { }
	public NemesisWeapon(WeaponCostume? costume = null) : base(costume) {
		slashAttack = new(this);
	}


	public override Vector3 GetTipPosition() => new(0f, 3.2f, 0f);

	public override IEnumerable<Action.Wrapper> GetAttacks(Entity target) {
		return [
			slashAttack
		];
	}


	public void HandlePlayer(Player player) {
		if (player.Entity is null) return;
		if (IsHolstered) return;

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