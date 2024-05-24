using System.Collections.Generic;
using Godot;
using KGySoft.CoreLibraries;
using SevenDev.Utility;

namespace LandlessSkies.Core;

public partial class CompositeChargeAttack(SingleWeapon weapon, CompositeChargeAttackBuilder info, IEnumerable<AttributeModifier> modifiers) : ChargeAttack(weapon, info.ChargeDuration) {
	private Entity? attributesTarget;


	protected override bool IsChargeStopped(InputDevice inputDevice) {
		return inputDevice.IsActionJustReleased(info.ActionKey);
	}


	protected override void ChargeDone(Entity entity) {
		GD.Print("Charged Up");
	}

	protected override void ChargedAttack(Entity entity) {
		QueueFree();
		entity.ExecuteAction(new AttackActionInfo(Weapon, info.ChargedAttack), true);
	}

	protected override void UnchargedAttack(Entity entity) {
		QueueFree();
		entity.ExecuteAction(new AttackActionInfo(Weapon, info.UnchargedAttack), true);
	}

	public override void HandlePlayer(Player player) {
		if (attributesTarget is null && player.Entity is Entity entity) {
			entity.AttributeModifiers.AddRange(modifiers);
			attributesTarget = entity;
		}
		base.HandlePlayer(player);
	}

	public override void _ExitTree() {
		base._ExitTree();
		attributesTarget?.AttributeModifiers.RemoveRange(modifiers);
	}
}