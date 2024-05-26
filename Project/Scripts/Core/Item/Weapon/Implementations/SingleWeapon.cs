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
		get => base.Skeleton;
		protected set {
			base.Skeleton = value;

			if (Model is IInjectable<Skeleton3D?> mSkeleton) mSkeleton.Inject(value);
		}
	}

	[Export] public override Handedness Handedness {
		get => base.Handedness;
		protected set {
			base.Handedness = value;

			if (Model is IInjectable<Handedness> mHanded) mHanded.Inject(value);
		}
	}



	public override int Style {
		get => _style;
		set => _style = value % (StyleCount + 1);
	}
	private int _style;

	public override List<ICustomizable> GetSubCustomizables() {
		List<ICustomizable> list = base.GetSubCustomizables();
		if (Model is not null) list.Add(Model);
		return list;
	}


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



	public override void Inject(Skeleton3D? skeleton) {
		base.Inject(skeleton);
		if (Model is IInjectable<Skeleton3D?> mSkeleton) mSkeleton.Inject(skeleton);
	}
	public override void Inject(Handedness handedness) {
		base.Inject(handedness);
		if (Model is IInjectable<Handedness> mHand) mHand.Inject(handedness);
	}

	public void Load(bool forceReload = false) {
		if (IsLoaded && ! forceReload) return;

		Unload();

		Model = Costume?.Instantiate()?.ParentTo(this);

		if (Model is null) return;

		if (Model is IInjectable<Skeleton3D?> mSkeleton) mSkeleton.Inject(Skeleton);
		if (Model is IInjectable<Handedness> mHanded) mHanded.Inject(Handedness);
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