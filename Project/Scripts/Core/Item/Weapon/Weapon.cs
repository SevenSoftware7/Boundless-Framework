namespace LandlessSkies.Core;

using System.Collections.Generic;
using System.Text.Json.Serialization;
using Godot;
using SevenDev.Utility;

[Tool]
[GlobalClass]
public abstract partial class Weapon : Node3D, IWeapon, IInjectable<Skeleton3D?>, IInjectable<Handedness>, ISaveable<Weapon>, ICustomizable {
	public static readonly Basis rightHandBoneBasis = Basis.FromEuler(new(Mathfs.Deg2Rad(-90f), 0f, Mathfs.Deg2Rad(90f)));
	public static readonly Basis leftHandBoneBasis = Basis.FromEuler(new(Mathfs.Deg2Rad(-90f), 0f, Mathfs.Deg2Rad(-90f)));


	[ExportGroup("Dependencies")]
	[Export] public virtual Handedness Handedness {
		get => _handedness;
		protected set => _handedness = value;
	}
	private Handedness _handedness = Handedness.Right;

	[Export] public virtual Skeleton3D? Skeleton {
		get => _skeleton;
		protected set => _skeleton = value;
	}
	private Skeleton3D? _skeleton;

	public virtual bool OnHand => true;


	public virtual int StyleCount { get; } = 1;
	public abstract int Style { get; set; }

	public abstract IWeapon.Type WeaponType { get; }
	public abstract IWeapon.Usage WeaponUsage { get; }
	public abstract IWeapon.Size WeaponSize { get; }


	public abstract string DisplayName { get; }
	public abstract Texture2D? DisplayPortrait { get; }


	public virtual List<ICustomization> GetCustomizations() => [];
	public virtual List<ICustomizable> GetSubCustomizables() => [];


	public abstract IEnumerable<AttackActionInfo> GetAttacks(Entity target);

	public abstract ISaveData<Weapon> Save();

	public virtual void Inject(Skeleton3D? skeleton) => Skeleton = skeleton;
	public virtual void Inject(Handedness handedness) => Handedness = handedness;


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