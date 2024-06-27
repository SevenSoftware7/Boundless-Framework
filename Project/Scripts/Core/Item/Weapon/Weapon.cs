namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Godot;
using SevenDev.Utility;

[Tool]
[GlobalClass]
public abstract partial class Weapon : Node3D, IWeapon, IUIObject, ICostumable, IPlayerHandler, IInjectable<Skeleton3D?>, IInjectable<Handedness>, IInjectable<WeaponHolsterState> {
	public static readonly Basis rightHandBoneBasis = Basis.FromEuler(new(Mathfs.Deg2Rad(-90f), 0f, Mathfs.Deg2Rad(-90f)));
	public static readonly Basis leftHandBoneBasis = Basis.FromEuler(new(Mathfs.Deg2Rad(-90f), 0f, Mathfs.Deg2Rad(90f)));


	public WeaponHolsterState HolsterState {
		get => _holsterState;
		set {
			_holsterState = value;
			if (_holsterState.IsHolstered) {
				CostumeHolder?.Disable();
			}
			else {
				CostumeHolder?.Enable();
			}
		}
	}
	private WeaponHolsterState _holsterState = WeaponHolsterState.Unholstered;

	[Export] protected bool IsHolstered {
		get => HolsterState.IsHolstered;
		private set => HolsterState = value;
	}

	public abstract WeaponType Type { get; }
	public abstract WeaponUsage Usage { get; }
	public abstract WeaponSize Size { get; }


	[Export] public AnimationLibrary? AnimationLibrary {
		get => _animationLibrary;
		set {
			_animationLibrary = value;
			LibraryName = _animationLibrary?.GetFileName() ?? "";
		}
	}
	private AnimationLibrary? _animationLibrary;
	protected StringName LibraryName { get; private set; } = "";

	private AnimationPlayer? animPlayer;


	[Export] private string _displayName = string.Empty;
	public string DisplayName => _displayName;

	public Texture2D? DisplayPortrait => CostumeHolder?.Costume?.DisplayPortrait;


	[ExportGroup("Costume")]
	[Export] public CostumeHolder? CostumeHolder { get; set; }


	[ExportGroup("Dependencies")]
	[Export] public virtual Handedness Handedness { get; protected set; }
	[Export] public virtual Skeleton3D? Skeleton { get; protected set; }

	public int Style {
		get => _style;
		set => _style = value % (StyleCount + 1);
	}
	private int _style;
	public virtual int StyleCount => 1;

	[Signal] public delegate void CostumeChangedEventHandler(WeaponCostume? newCostume, WeaponCostume? oldCostume);


	protected Weapon() : base() { }
	public Weapon(WeaponCostume? costume = null) {
		CostumeHolder = new CostumeHolder(costume).ParentTo(this);
	}


	public virtual List<ICustomization> GetCustomizations() => [];
	public List<ICustomizable> GetSubCustomizables() {
		List<ICustomizable> list = [];
		if (CostumeHolder?.Model is not null) list.Add(CostumeHolder.Model);
		return list;
	}

	public abstract IEnumerable<AttackBuilder> GetAttacks(Entity target);

	public ISaveData<IWeapon> Save() => new WeaponSaveData<Weapon>(this);

	public virtual void Inject(Skeleton3D? skeleton) => Skeleton = skeleton;
	public virtual void Inject(Handedness handedness) => Handedness = handedness;
	public virtual void Inject(WeaponHolsterState value) => HolsterState = value;


	public virtual void HandlePlayer(Player player) {
		if (animPlayer is null && player.Entity?.AnimationPlayer is not null && AnimationLibrary is not null) {
			animPlayer = player.Entity.AnimationPlayer;

			GD.Print($"Adding Library {LibraryName}");
			if (! animPlayer.HasAnimationLibrary(LibraryName)) {
				animPlayer.AddAnimationLibrary(LibraryName, AnimationLibrary);
			}
		}
	}
	public virtual void DisavowPlayer() {
		if (animPlayer is not null && AnimationLibrary is not null) {
			if (animPlayer.HasAnimationLibrary(LibraryName)) {
				animPlayer.RemoveAnimationLibrary(LibraryName);
			}

			animPlayer = null;
		}
	}


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


	public override void _Notification(int what) {
		base._Notification(what);
		switch ((ulong) what) {
		case NotificationPathRenamed:
			if (IsNodeReady()) {
				this.RequestInjection<Skeleton3D?>();
				this.RequestInjection<Handedness>();
			}
			break;
		}
	}

	public override void _Process(double delta) {
		base._Process(delta);

		if (/* OnHand */true) {
			StickToSkeletonBone();
		}
	}


	[Serializable]
	public class WeaponSaveData<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(T weapon) : CostumableSaveData<T, WeaponCostume>(weapon) where T : Weapon {

	}
}