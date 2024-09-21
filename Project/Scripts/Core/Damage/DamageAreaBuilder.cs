namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;

[GlobalClass]
public sealed partial class DamageAreaBuilder : Resource {
	[Export] public ulong LifeTime = 250;

	[Export] public float Damage = 1f;
	[Export] public DamageArea.DamageType Type = DamageArea.DamageType.Physical;

	[Export] public bool SelfDamage = false;

	[Export] public bool CanParry = false;
	[Export] public bool Parriable = false;

	[Export] public bool Attached = false;
	[Export] public HitBoxAttachment AttachmentTarget = HitBoxAttachment.Entity;

	[Export] public Godot.Collections.Array<DamageHitboxBuilder> HitboxBuilders = [];


	public DamageArea Build(Attack attack) {
		DamageArea damageArea = new() {
			DamageDealer = attack,
			LifeTime = LifeTime,
			Damage = Damage,
			Type = Type,
			SelfDamage = SelfDamage,
			CanParry = CanParry,
			Parriable = Parriable,
		};

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
			damageArea.ParentTo(parent);
			damageArea.Transform = new(Basis.Identity, offset);
		}
		else {
			damageArea.ParentTo(attack.GetTree().Root);
			damageArea.GlobalTransform = parent.GlobalTransform.TranslatedLocal(offset);
		}

		foreach (DamageHitboxBuilder hitboxBuilder in HitboxBuilders) {
			hitboxBuilder.Build(damageArea);
		}

		return damageArea;
	}



	[System.Serializable]
	public enum HitBoxAttachment {
		Weapon = 0,
		WeaponTip = 1,
		Entity = 2,
		EntityCenterOfMass = 3,
	}
}