namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using SevenDev.Utility;

[Tool]
[GlobalClass]
public abstract partial class SingleWeapon : Weapon, IUIObject {
	[Export] private string _displayName = string.Empty;
	public override string DisplayName => _displayName;
	public override Texture2D? DisplayPortrait => Costume?.DisplayPortrait;


	[ExportGroup("Costume")]
	[Export] public WeaponCostume? Costume {
		get => _costume;
		private set {
			if (this.IsInitializationSetterCall()) {
				_costume = value;
				return;
			}

			SetCostume(value);
		}
	}
	private WeaponCostume? _costume;

	[Export] protected Model? Model { get; private set; }


	[ExportGroup("Dependencies")]
	[Export] public override Skeleton3D? Skeleton {
		get => _skeleton;
		protected set {
			_skeleton = value;

			if (this.IsInitializationSetterCall()) return;

			if (Model is ISkeletonAdaptable mSkeleton) mSkeleton.SetParentSkeleton(value);
		}
	}
	private Skeleton3D? _skeleton;

	[Export] public override Handedness Handedness {
		get => _handedness;
		protected set {
			_handedness = value;

			if (this.IsInitializationSetterCall()) return;

			if (Model is IHandAdaptable mHanded) mHanded.SetHandedness(value);
		}
	}
	private Handedness _handedness = Handedness.Right;



	public override int Style {
		get => _style;
		set => _style = value % (StyleCount + 1);
	}
	private int _style;

	public override ICustomizable[] Customizables => [.. new List<ICustomizable?>(){Model}.OfType<ICustomizable>()];


	[Signal] public delegate void CostumeChangedEventHandler(WeaponCostume? newCostume, WeaponCostume? oldCostume);


	protected SingleWeapon() : base() { }
	public SingleWeapon(WeaponCostume? costume) : this() {
		SetCostume(costume);
	}



	public void SetCostume(WeaponCostume? newCostume) {
		WeaponCostume? oldCostume = _costume;
		if (newCostume == oldCostume) return;

		_costume = newCostume;

		Load(true);

		EmitSignal(SignalName.CostumeChanged, newCostume!, oldCostume!);
	}



	public override void SetParentSkeleton(Skeleton3D? skeleton) {
		base.SetParentSkeleton(skeleton);
		if (Model is ISkeletonAdaptable mSkeleton) mSkeleton.SetParentSkeleton(skeleton);
	}
	public override void SetHandedness(Handedness handedness) {
		base.SetHandedness(handedness);
		if (Model is IHandAdaptable mHand) mHand.SetHandedness(handedness);
	}

	protected void Load(bool forceReload = false) {
		if (Model is not null && !forceReload) return;

		Model?.QueueFree();
		Model = Costume?.Instantiate()?.SetParentToSceneInstance(this);

		if (Model is null) return;

		if (Model is ISkeletonAdaptable mSkeleton) mSkeleton.SetParentSkeleton(Skeleton);
		if (Model is IHandAdaptable mHanded) mHanded.SetHandedness(Handedness);
	}
	protected void Unload() {
		Model?.QueueFree();
		Model = null;
	}

	public override void _Notification(int what) {
		base._Notification(what);
		switch ((ulong)what) {
			case NotificationSceneInstantiated:
				Callable.From(() => Load()).CallDeferred();
				break;
		}
	}

	public override ISaveData<Weapon> Save() {
		return new SingleWeaponSaveData(this);
	}

	[Serializable]
	protected class SingleWeaponSaveData(SingleWeapon weapon) : ISaveData<Weapon> {
		public string TypeName { get; set; } = weapon.GetType().Name;
		public Weapon Load() {
			return null!;
		}
	}
}