namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using Godot;
using SevenDev.Boundless.Utility;
using SevenDev.Boundless.Injection;

/// <summary>
/// A Weapon is needed to initiate an Attack as an Entity.
/// </summary>
[Tool]
[GlobalClass]
public abstract partial class Weapon : Node3D, IWeapon, IItem<Weapon>, IUIObject, ICostumable, IDamageDealerProxy {
	private static readonly StringName LeftWeapon = "LeftWeapon";
	private static readonly StringName RightWeapon = "RightWeapon";
	private static readonly StringName LeftHand = "LeftHand";
	private static readonly StringName RightHand = "RightHand";

	public static readonly Basis rightHandBoneBasis = Basis.FromEuler(new(Mathf.DegToRad(-90f), 0f, Mathf.DegToRad(-90f)));
	public static readonly Basis leftHandBoneBasis = Basis.FromEuler(new(Mathf.DegToRad(-90f), 0f, Mathf.DegToRad(90f)));


	[Export] public WeaponResourceDataKey ResourceKeyProvider { get; private set; } = new();
	IDataKeyProvider<Weapon> IItem<Weapon>.KeyProvider => ResourceKeyProvider;

	[Export] public ItemUIData? UI { get; private set; }
	public string DisplayName => UI?.DisplayName ?? string.Empty;
	public Texture2D? DisplayPortrait => UI?.DisplayPortrait;


	[Injectable]
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


	public IDamageDealer? Sender => Entity;


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



	[ExportGroup("Costume")]
	[Export] public CostumeHolder? CostumeHolder { get; set; }


	[ExportGroup("Dependencies")]

	public Entity? Entity {
		get => _entity;
		protected set {
			_entity = value;
			Skeleton = value?.Skeleton;
			Handedness = value?.Handedness ?? default;
		}
	}
	private Entity? _entity;
	[Injectable] public virtual Skeleton3D? Skeleton { get; protected set; }
	[Injectable] public virtual Handedness Handedness { get; protected set; }

	public uint Style {
		get => _style;
		set => _style = value % (MaxStyle + 1);
	}
	private uint _style;
	public virtual uint MaxStyle { get; } = 0;


	protected Weapon() : this(null) { }
	public Weapon(IItemData<Costume>? costume = null) : base() {
		if (CostumeHolder is null) {
			CostumeHolder = new CostumeHolder(costume).ParentTo(this);
		}
		else {
			CostumeHolder.SetCostume(costume);
		}
	}

	public abstract Vector3 GetTipPosition();


	public virtual List<ICustomization> GetCustomizations() => [];
	public List<ICustomizable> GetSubCustomizables() {
		List<ICustomizable> list = [];
		if (CostumeHolder?.Costume is not null) list.Add(CostumeHolder.Costume);
		return list;
	}

	public abstract IEnumerable<Action.Wrapper> GetAttacks(Entity target);

	public IPersistenceData<IWeapon> Save() => new WeaponSaveData<Weapon>(this);


	[Injectable]
	public virtual void Inject(Entity? entity) {
		if (AnimationLibrary is null || entity == Entity) {
			Entity = entity;
			return;
		}

		AnimationPlayer? animPlayer = Entity?.AnimationPlayer;
		if (animPlayer is not null && animPlayer.HasAnimationLibrary(LibraryName)) {
			animPlayer.Stop(); // Prevents AccessViolationException
			GD.Print($"Removing Library {LibraryName}");
			animPlayer.RemoveAnimationLibrary(LibraryName);
		}

		Entity = entity;

		animPlayer = Entity?.AnimationPlayer;
		if (animPlayer is not null && !animPlayer.HasAnimationLibrary(LibraryName)) {
			GD.Print($"Adding Library {LibraryName}");
			animPlayer.AddAnimationLibrary(LibraryName, AnimationLibrary);
		}
	}
	public virtual void RequestInjection() {
		if (!this.RequestInjection<Entity>()) {
			_entity = null;

			if (!this.RequestInjection<Skeleton3D>()) Skeleton = null;
			if (!this.RequestInjection<Handedness>()) Handedness = Handedness.Right;
		}
	}


	private void StickToSkeletonBone() {
		if (Skeleton is null) return;

		(string boneName, string fallbackBoneName) = Handedness switch {
			Handedness.Left => (LeftWeapon, LeftHand),
			Handedness.Right or _ => (RightWeapon, RightHand),
		};

		if (Skeleton.TryGetBoneTransform(boneName, out Transform3D weaponBoneTransform)) {
			GlobalTransform = weaponBoneTransform;
			return;
		}

		if (Skeleton.TryGetBoneTransform(fallbackBoneName, out Transform3D handBoneTransform)) {
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
	public class WeaponSaveData<T>(T weapon) : ItemPersistenceData<T>(weapon) where T : Weapon, IItem<T>;
}