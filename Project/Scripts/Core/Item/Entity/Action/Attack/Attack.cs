namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using Godot;
using SevenDev.Utility;

public abstract partial class Attack(Entity entity, Weapon Weapon) : EntityAction(entity) {
	public Weapon Weapon { get; private set; } = Weapon;

	protected static StringName GetAnimationPath(StringName library, StringName attack) => library.IsEmpty ? attack : $"{library}/{attack}";

	public void CreateHitBox(float damage, Vector3 size, Vector3 position, bool parented, ulong lifeTime = 0) {
		DamageArea3D hitArea = CreateHurtArea(damage, lifeTime);
		if (parented) {
			hitArea.ParentTo(Weapon);
			hitArea.Transform = new() {
				Origin = position,
				Basis = Basis.Identity
			};
		}
		else {
			hitArea.GlobalTransform = new() {
				Origin = Weapon.GlobalTransform * position,
				Basis = Weapon.GlobalBasis
			};
		}

		AddCollisionShapes(hitArea, size);
	}


	public virtual DamageArea3D CreateHurtArea(float damage, ulong lifeTime) {
		return new(Entity as IDamageDealer, damage, lifeTime);
	}

	public virtual void AddCollisionShapes(DamageArea3D damageArea, Vector3 size) {
		new CollisionShape3D() {
			Shape = new BoxShape3D() {
				Size = size,
			}
		}.ParentTo(damageArea);

		new MeshInstance3D() {
			Mesh = new BoxMesh() {
				Size = size
			}
		}.ParentTo(damageArea);
	}



	[Flags]
	public enum AttackType : byte {
		Melee = 1 << 0,
		Projectile = 1 << 1,
		Hitscan = 1 << 2,
		Parry = 1 << 3,
		Explosive = 1 << 4,
	}


	public static class Comparers {
		public static readonly IComparer<AttackInfo> PureDamage = new ComparisonComparer<AttackInfo>(
			(a, b) => a?.PotentialDamage.CompareTo(b?.PotentialDamage ?? 0) ?? 0
		);
	}
}


public static class AttackExtensions {
	public static AttackInfo SelectAttack(this AttackInfo[] attacks, IComparer<AttackInfo> priority, float skillLevel = 0.5f) {
		skillLevel = Mathf.Clamp(skillLevel, 0, 1);

		Array.Sort(attacks, priority);

		float skillMargin = skillLevel * attacks.Length;
		int weightedIndex = Mathf.RoundToInt(GD.Randf() * skillMargin);

		return attacks[weightedIndex];
	}
}