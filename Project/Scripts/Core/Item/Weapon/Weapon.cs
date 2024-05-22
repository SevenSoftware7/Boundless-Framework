namespace LandlessSkies.Core;

using System.Collections.Generic;
using System.Text.Json.Serialization;
using Godot;
using SevenDev.Utility;

[Tool]
[GlobalClass]
public abstract partial class Weapon : Node3D, IWeapon, ISkeletonAdaptable {
	public static readonly Basis rightHandBoneBasis = Basis.FromEuler(new(Mathfs.Deg2Rad(-90f), 0f, Mathfs.Deg2Rad(90f)));
	public static readonly Basis leftHandBoneBasis = Basis.FromEuler(new(Mathfs.Deg2Rad(-90f), 0f, Mathfs.Deg2Rad(-90f)));


	public virtual int StyleCount { get; } = 1;
	public abstract int Style { get; set; }

	public virtual bool OnHand => true;

	public abstract IWeapon.Type WeaponType { get; }
	public abstract IWeapon.Usage WeaponUsage { get; }
	public abstract IWeapon.Size WeaponSize { get; }
	public abstract Handedness Handedness { get; protected set; }

	public abstract Skeleton3D? Skeleton { get; protected set; }


	public abstract string DisplayName { get; }
	public abstract Texture2D? DisplayPortrait { get; }

	public virtual ICustomizable[] Customizables => [];
	public virtual ICustomization[] Customizations => [];


	public abstract IEnumerable<AttackActionInfo> GetAttacks(Entity target);
	public void Inject(Entity? entity) {
		SetParentSkeleton(entity?.Skeleton);
		SetHandedness(entity?.Handedness ?? Handedness.Right);
	}


	public virtual void HandlePlayer(Player player) { }
	public virtual void DisavowPlayer(Player player) { }

	public abstract ISaveData<Weapon> Save();

	public virtual void SetParentSkeleton(Skeleton3D? skeleton) => Skeleton = skeleton;
	public virtual void SetHandedness(Handedness handedness) => Handedness = handedness;

	private void StickToSkeletonBone() {
		if (Skeleton is null) return;


		string boneName = Handedness switch {
			Handedness.Left => "LeftWeapon",
			Handedness.Right or _ => "RightWeapon",
		};
		if (Skeleton.TryGetBoneTransform(boneName, out Transform3D weaponBoneTransform)) {
			GlobalTransform = weaponBoneTransform;
			return;
		}

		boneName = Handedness switch {
			Handedness.Left => "LeftHand",
			Handedness.Right or _ => "RightHand",
		};
		if (Skeleton.TryGetBoneTransform(boneName, out Transform3D handBoneTransform)) {
			GlobalTransform = handBoneTransform with { Basis = handBoneTransform.Basis * (Handedness == Handedness.Right ? rightHandBoneBasis : leftHandBoneBasis) };
			return;
		}

		GlobalTransform = Skeleton.GlobalTransform;
	}



	public override void _Process(double delta) {
		base._Process(delta);

		if (OnHand) {
			StickToSkeletonBone();
		}
	}
}