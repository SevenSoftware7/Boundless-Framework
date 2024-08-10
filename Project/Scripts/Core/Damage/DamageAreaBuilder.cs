namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;

[GlobalClass]
public sealed partial class DamageAreaBuilder : Resource {
	[Export] public float Damage = 1f;
	[Export] public ulong LifeTime = 250;

	[Export] public bool Parriable = false;
	[Export] public bool CanParry = false;

	[Export] public bool Attached = false;
	[Export] public HitBoxAttachment AttachmentTarget = HitBoxAttachment.Entity;

	[Export] public Godot.Collections.Array<DamageHitboxBuilder> HitboxBuilders = [];


	public DamageArea Build(Attack attack) {
		DamageArea damageArea = new(LifeTime) {
			Damage = Damage,
			DamageDealer = attack.Entity as IDamageDealer,
			Parriable = Parriable,
			CanParry = CanParry,
		};

		Node3D parent = AttachmentTarget switch {
			HitBoxAttachment.Weapon or HitBoxAttachment.WeaponTip => attack.Weapon,
			HitBoxAttachment.Entity or _ => attack.Entity,
		};

		Vector3 offset = AttachmentTarget switch {
			HitBoxAttachment.WeaponTip => attack.Weapon.GetTipPosition(),
			_ => Vector3.Zero,
		};

		if (Attached) {
			damageArea.ParentTo(parent);
			damageArea.Transform = new() {
				Origin = offset,
				Basis = Basis.Identity
			};
		}
		else {
			damageArea.ParentTo(attack.GetTree().Root);
			damageArea.GlobalTransform = new() {
				Origin = parent.GlobalTransform * offset,
				Basis = parent.GlobalBasis
			};
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
	}
}