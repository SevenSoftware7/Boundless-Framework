namespace LandlessSkies.Core;

using System;
using Godot;
using SevenDev.Utility;

[GlobalClass]
public sealed partial class AttackDamageAreaBuilder : DamageAreaBuilder<Attack> {
	[Export] public bool Attached = false;
	[Export] public HitBoxAttachment AttachmentTarget = HitBoxAttachment.Entity;



	protected override void SetupDamageArea(Attack attack, DamageArea area) {
		base.SetupDamageArea(attack, area);
		Node3D parent = AttachmentTarget switch {
			HitBoxAttachment.Weapon or HitBoxAttachment.WeaponTip => attack.Weapon,
			HitBoxAttachment.Entity or HitBoxAttachment.EntityCenterOfMass or _ => attack.Entity,
		};

		Vector3 offset = AttachmentTarget switch {
			HitBoxAttachment.WeaponTip => attack.Weapon.GetTipPosition(),
			HitBoxAttachment.EntityCenterOfMass => attack.Entity.CenterOfMass is null
				? Vector3.Zero
				: attack.Entity.GlobalTransform.Inverse() * attack.Entity.CenterOfMass.GlobalPosition,
			_ => Vector3.Zero,
		};

		if (Attached) {
			area.ParentTo(parent);
			area.Transform = new(Basis.Identity, offset);
		}
		else {
			area.ParentTo(attack.GetTree().Root);
			area.GlobalTransform = parent.GlobalTransform.TranslatedLocal(offset);
		}
	}


	[Serializable]
	public enum HitBoxAttachment {
		Weapon = 0,
		WeaponTip = 1,
		Entity = 2,
		EntityCenterOfMass = 3,
	}
}