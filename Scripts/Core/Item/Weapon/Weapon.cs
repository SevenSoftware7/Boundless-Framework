namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using Godot;
using SevenDev.Boundless.Utility;
using SevenDev.Boundless.Injection;
using SevenDev.Boundless.Persistence;

/// <summary>
/// A Weapon is needed to initiate an Attack as an Entity.
/// </summary>
[Tool]
[GlobalClass]
public abstract partial class Weapon : Node3D, IWeapon, IItem<Weapon>, IUIObject, ICostumable, IDamageDealerProxy, IPersistent<Weapon> {
	private static readonly StringName LeftWeapon = "LeftWeapon";
	private static readonly StringName RightWeapon = "RightWeapon";
	private static readonly StringName LeftHand = "LeftHand";
	private static readonly StringName RightHand = "RightHand";

	public static readonly Basis rightHandBoneBasis = Basis.FromEuler(new(Mathf.DegToRad(-90f), 0f, Mathf.DegToRad(-90f)));
	public static readonly Basis leftHandBoneBasis = Basis.FromEuler(new(Mathf.DegToRad(-90f), 0f, Mathf.DegToRad(90f)));


	public IInjectionNode InjectionNode { get; }

	IItemKeyProvider<Weapon> IItem<Weapon>.KeyProvider => KeyProvider;
	[Export] private WeaponResourceItemKey KeyProvider = new();

	[Export] public ItemUIData? UI { get; private set; }
	public string DisplayName => UI?.DisplayName ?? string.Empty;
	public Texture2D? DisplayPortrait => UI?.DisplayPortrait;

	public IDamageDealer? Sender => Entity;


	[Injectable]
	public WeaponHolsterState HolsterState {
		get => _holsterState;
		set {
			_holsterState = value;
			if (_holsterState.IsHolstered) {
				_costumeHolder?.Disable();
			}
			else {
				_costumeHolder?.Enable();
			}
		}
	}
	private WeaponHolsterState _holsterState = WeaponHolsterState.Unholstered;

	[Export]
	private bool IsHolstered {
		get => HolsterState.IsHolstered;
		set => HolsterState = value;
	}

	[Injectable]
	public StyleState Style {
		get => _style;
		set => _style = value.WrappedWith(MaxStyle);
	}
	private StyleState _style;
	public virtual StyleState MaxStyle { get; } = StyleState.Primary;

	public abstract IWeapon.WeaponKind Kind { get; }
	public abstract IWeapon.WeaponUsage Usage { get; }
	public abstract IWeapon.WeaponSize Size { get; }


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
	public CostumeHolder CostumeHolder => _costumeHolder ??= new CostumeHolder().SafeReparentAndSetOwner(this).SafeRename(nameof(CostumeHolder));
	[Export] private CostumeHolder? _costumeHolder;


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


	public Weapon() : base() {
		InjectionNode = new GodotNodeInjectionNode(this);
	}


	public abstract Vector3 GetTipPosition();


	public virtual IEnumerable<IUIObject> GetSubObjects() => [CostumeHolder];
	public virtual Dictionary<string, ICustomization> GetCustomizations() => [];

	public abstract IEnumerable<Action.Wrapper> GetAttacks(Entity target);


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

	public virtual IPersistenceData<Weapon> Save() => new WeaponSaveData<Weapon>(this);
	[Serializable]
	public class WeaponSaveData<T>(T weapon) : ItemPersistenceData<T>(weapon) where T : Weapon, IItem<T>;
}