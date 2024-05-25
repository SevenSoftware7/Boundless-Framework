namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using SevenDev.Utility;

[Tool]
[GlobalClass]
public abstract partial class SingleWeapon : Weapon, ICostumable<WeaponCostume>, IUIObject {
	[Export] private string _displayName = string.Empty;
	public override string DisplayName => _displayName;
	public override Texture2D? DisplayPortrait => Costume?.DisplayPortrait;


	[ExportGroup("Costume")]
	[Export] public WeaponCostume? Costume {
		get => _costume;
		set => SetCostume(value);
	}
	private WeaponCostume? _costume;

	public Model? Model { get; private set; }
	public bool IsLoaded => Model is not null;


	[ExportGroup("Dependencies")]
	[Export] public override Skeleton3D? Skeleton {
		get => _skeleton;
		protected set {
			_skeleton = value;

			if (Model is ISkeletonAdaptable mSkeleton) mSkeleton.SetParentSkeleton(value);
		}
	}
	private Skeleton3D? _skeleton;

	[Export] public override Handedness Handedness {
		get => _handedness;
		protected set {
			_handedness = value;

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
		EmitSignal(SignalName.CostumeChanged, newCostume!, oldCostume!);

		Callable.From<bool>(Load).CallDeferred(true);
	}



	public override void SetParentSkeleton(Skeleton3D? skeleton) {
		base.SetParentSkeleton(skeleton);
		if (Model is ISkeletonAdaptable mSkeleton) mSkeleton.SetParentSkeleton(skeleton);
	}
	public override void SetHandedness(Handedness handedness) {
		base.SetHandedness(handedness);
		if (Model is IHandAdaptable mHand) mHand.SetHandedness(handedness);
	}

	public void Load(bool forceReload = false) {
		if (IsLoaded && !forceReload) return;

		Unload();

		Model = Costume?.Instantiate()?.ParentTo(this);

		if (Model is null) return;

		if (Model is ISkeletonAdaptable mSkeleton) mSkeleton.SetParentSkeleton(Skeleton);
		if (Model is IHandAdaptable mHanded) mHanded.SetHandedness(Handedness);
	}
	public void Unload() {
		Model?.QueueFree();
		Model = null;
	}

	public override ISaveData<Weapon> Save() => new SingleWeaponSaveData<SingleWeapon>(this);



	[Serializable]
	public class SingleWeaponSaveData<T>(T weapon) : CostumableSaveData<Weapon, T, WeaponCostume>(weapon) where T : SingleWeapon {
		// protected override WeaponCostume? GetCostume(T data) => data.Costume;
		// protected override void SetCostume(T data, WeaponCostume? costume) => data.Costume = costume;


		// public override SingleWeapon? Load() {
		// 	if (base.Load() is not SingleWeapon weapon) return null;

		// 	return base.Load();
		// }
	}
}