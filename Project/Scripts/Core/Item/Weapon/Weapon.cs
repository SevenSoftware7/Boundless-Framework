namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Godot;
using SevenDev.Utility;

/// <summary>
/// A Weapon is needed to initiate an Attack as an Entity.
/// </summary>
[Tool]
[GlobalClass]
public abstract partial class Weapon : Node3D, IWeapon, IUIObject, ICostumable, IInjectable<Entity?>, IInjectable<Skeleton3D?>, IInjectable<Handedness>, IInjectable<WeaponHolsterState> {
	private static readonly StringName LeftWeapon = "LeftWeapon";
	private static readonly StringName RightWeapon = "RightWeapon";
	private static readonly StringName LeftHand = "LeftHand";
	private static readonly StringName RightHand = "RightHand";

	public static readonly Basis rightHandBoneBasis = Basis.FromEuler(new(Mathf.DegToRad(-90f), 0f, Mathf.DegToRad(-90f)));
	public static readonly Basis leftHandBoneBasis = Basis.FromEuler(new(Mathf.DegToRad(-90f), 0f, Mathf.DegToRad(90f)));

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

	[Export]
	protected bool IsHolstered {
		get => HolsterState.IsHolstered;
		private set => HolsterState = value;
	}

	public abstract WeaponType Type { get; }
	public abstract WeaponUsage Usage { get; }
	public abstract WeaponSize Size { get; }



	[Export]
	public AnimationLibrary? AnimationLibrary {
		get => _animationLibrary;
		set {
			_animationLibrary = value;
			LibraryName = _animationLibrary?.ResourceName ?? string.Empty;
		}
	}
	private AnimationLibrary? _animationLibrary;

	public StringName LibraryName { get; protected set; } = string.Empty;



	[Export] private string _displayName = string.Empty;
	public string DisplayName => _displayName;

	public Texture2D? DisplayPortrait => CostumeHolder?.Costume?.DisplayPortrait;


	[ExportGroup("Costume")]
	[Export] public CostumeHolder? CostumeHolder { get; set; }


	[ExportGroup("Dependencies")]

	public virtual Entity? Entity { get; protected set; }
	public virtual Skeleton3D? Skeleton { get; protected set; }
	public virtual Handedness Handedness { get; protected set; }

	public int Style {
		get => _style;
		set => _style = value % (StyleCount + 1);
	}
	private int _style;
	public virtual int StyleCount => 1;

	[Signal] public delegate void CostumeChangedEventHandler(WeaponCostume? newCostume, WeaponCostume? oldCostume);


	protected Weapon() : this(null) { }
	public Weapon(WeaponCostume? costume = null) : base() {
		CostumeHolder = new CostumeHolder(costume).ParentTo(this);
	}

	public abstract Vector3 GetTipPosition();


	public virtual List<ICustomization> GetCustomizations() => [];
	public List<ICustomizable> GetSubCustomizables() {
		List<ICustomizable> list = [];
		if (CostumeHolder?.Model is not null) list.Add(CostumeHolder.Model);
		return list;
	}

	public abstract IEnumerable<Attack.Wrapper> GetAttacks(Entity target);

	public ISaveData<IWeapon> Save() => new WeaponSaveData<Weapon>(this);

	public virtual void Inject(Entity? entity) {
		if (AnimationLibrary is null || entity == Entity) {
			Entity = entity;
			Inject(entity?.Skeleton);
			Inject(entity?.Handedness ?? default);
			return;
		}

		AnimationPlayer? animPlayer = Entity?.AnimationPlayer;
		if (animPlayer is not null && animPlayer.HasAnimationLibrary(LibraryName)) {
			animPlayer.Stop(); // Prevents AccessViolationException
			GD.Print($"Removing Library {LibraryName}");
			animPlayer.RemoveAnimationLibrary(LibraryName);
		}

		Entity = entity;
		Inject(entity?.Skeleton);
		Inject(entity?.Handedness ?? default);

		animPlayer = Entity?.AnimationPlayer;
		if (animPlayer is not null && !animPlayer.HasAnimationLibrary(LibraryName)) {
			GD.Print($"Adding Library {LibraryName}");
			animPlayer.AddAnimationLibrary(LibraryName, AnimationLibrary);
		}
	}
	public virtual void Inject(Skeleton3D? skeleton) => Skeleton = skeleton;
	public virtual void Inject(Handedness handedness) => Handedness = handedness;
	public virtual void Inject(WeaponHolsterState value) => HolsterState = value;
	public virtual void RequestInjection() {
		if (!this.RequestInjection<Entity?>()) {
			this.RequestInjection<Skeleton3D?>();
			this.RequestInjection<Handedness>();
		}
	}


	private void StickToSkeletonBone() {
		if (Skeleton is null) return;


		string boneName = Handedness switch {
			Handedness.Left => LeftWeapon,
			Handedness.Right or _ => RightWeapon,
		};
		if (Skeleton.TryGetBoneTransform(boneName, out Transform3D weaponBoneTransform)) {
			GlobalTransform = weaponBoneTransform;
			return;
		}

		boneName = Handedness switch {
			Handedness.Left => LeftHand,
			Handedness.Right or _ => RightHand,
		};
		if (Skeleton.TryGetBoneTransform(boneName, out Transform3D handBoneTransform)) {
			GlobalTransform = handBoneTransform with { Basis = handBoneTransform.Basis * (Handedness == Handedness.Right ? rightHandBoneBasis : leftHandBoneBasis) };
			return;
		}

		GlobalTransform = Skeleton.GlobalTransform;
	}

	public override void _Ready() {
		base._Ready();

		if (GetParent()?.IsNodeReady() ?? false) {
			RequestInjection();
		}
	}

	public override void _Process(double delta) {
		base._Process(delta);
			RequestReady();

		if (/* OnHand */true) {
			StickToSkeletonBone();
		}
	}


	[Serializable]
	public class WeaponSaveData<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(T weapon) : CostumableSaveData<T, WeaponCostume>(weapon) where T : Weapon;
}