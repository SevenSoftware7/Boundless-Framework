namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using Godot;
using SevenDev.Utility;

/// <summary>
/// An Attack that an Entity can execute, using a given Weapon.
/// </summary>
/// <param name="entity">Inherited from <see cref="Action"/>.</param>
/// <param name="weapon">The Weapon which will be used in the Attack.</param>
/// <param name="modifiers">Inherited from <see cref="Action"/>.</param>
public abstract partial class Attack(Entity entity, Weapon weapon, AnimationPath path, IEnumerable<AttributeModifier>? modifiers = null) : AnimationAction(entity, path, modifiers), IDamageDealerProxy {
	private readonly List<DamageArea?> damageAreas = [];

	public Weapon Weapon { get; private set; } = weapon;
	public IDamageDealer? Sender => Weapon;


	public void CreateHitBox(AttackDamageAreaBuilder builder) {
		DamageArea hitArea = builder.Build(this);

		damageAreas.Add(hitArea);
		hitArea.OnDestroy += () => {
			int index = damageAreas.IndexOf(hitArea);
			if (index < 0) return;

			damageAreas[index] = null!; // Replace by null to keep the index order intact
		};
	}

	/// <summary>
	/// Method to enable or disable an Attack hitbox's Parriability, mainly for Animations, but can be used via script too.
	/// </summary>
	/// <param name="canBeParried">Whether the Attack hitbox should be parriable</param>
	public void SetCanBeParried(bool canBeParried, int damageAreaIndex) {
		if (damageAreaIndex > damageAreas.Count) return;
		DamageArea? damageArea = damageAreas[damageAreaIndex];
		if (damageArea is null) return;

		if (canBeParried) {
			damageArea.Flags |= IDamageDealer.DamageFlags.CanBeParried;
		}
		else {
			damageArea.Flags &= ~IDamageDealer.DamageFlags.CanBeParried;
		}

		_SetCanBeParried(canBeParried, damageArea);
	}
	/// <summary>
	/// Callback method when updating the Parriability of an Attack hitbox
	/// </summary>
	/// <param name="canBeParried">Whether the attack hitbox was set to be parriable or not</param>
	/// <param name="damageArea">The DamageArea which had its parriability changed</param>
	protected virtual void _SetCanBeParried(bool canBeParried, DamageArea damageArea) { }

	/// <summary>
	/// Method to enable or disable an Attack hitbox's ability to parry others, mainly for Animations, but can be used via script too.
	/// </summary>
	/// <param name="canParry">Whether the Attack hitbox should be able to Parry other Attacks</param>
	public void SetCanParry(bool canParry, int damageAreaIndex) {
		if (damageAreaIndex > damageAreas.Count) return;
		DamageArea? damageArea = damageAreas[damageAreaIndex];
		if (damageArea is null) return;

		if (canParry) {
			damageArea.Flags |= IDamageDealer.DamageFlags.CanParry;
		}
		else {
			damageArea.Flags &= ~IDamageDealer.DamageFlags.CanParry;
		}

		_SetCanParry(canParry, damageArea);
	}
	/// <summary>
	/// Callback method when updating the ability of an Attack hitbox to Parry other Attacks
	/// </summary>
	/// <param name="canParry">Whether the attack hitbox was set to be able to Parry or not</param>
	/// <param name="damageArea">The DamageArea which had its ability to Parry changed</param>
	protected virtual void _SetCanParry(bool canParry, DamageArea damageArea) { }


	[Flags]
	public enum AttackType : byte {
		Melee = 1 << 0,
		Projectile = 1 << 1,
		Hitscan = 1 << 2,
		Parry = 1 << 3,
		Explosive = 1 << 4,
	}


	public static class Comparers {
		public static readonly IComparer<Builder> PureDamage = new ComparisonComparer<Builder>(
			(a, b) => a?.PotentialDamage.CompareTo(b?.PotentialDamage ?? 0) ?? 0
		);
	}



	public new abstract class Builder(Weapon weapon) : Action.Builder() {
		public readonly Weapon Weapon = weapon;
		public float PotentialDamage { get; }
		public AttackType Type { get; }


		public abstract override Attack Build(Entity entity);
	}
}


public static class AttackExtensions {
	public static Attack.Builder SelectAttack(this Attack.Builder[] attacks, IComparer<Attack.Builder> priority, float skillLevel = 0.5f) {
		skillLevel = Mathf.Clamp(skillLevel, 0, 1);

		Array.Sort(attacks, priority);

		float skillMargin = skillLevel * attacks.Length;
		int weightedIndex = Mathf.RoundToInt(GD.Randf() * skillMargin);

		return attacks[weightedIndex];
	}
}