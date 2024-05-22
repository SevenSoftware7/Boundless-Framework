namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
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
		private set => SetCostume(value);
	}
	private WeaponCostume? _costume;

	protected Model? Model { get; private set; }


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


		if (Engine.IsEditorHint()) {
			Callable.From<bool>(Load).CallDeferred(true);
		} else {
			Load(true);
		}
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
		Model = Costume?.Instantiate()?.ParentTo(this);

		if (Model is null) return;

		if (Model is ISkeletonAdaptable mSkeleton) mSkeleton.SetParentSkeleton(Skeleton);
		if (Model is IHandAdaptable mHanded) mHanded.SetHandedness(Handedness);
	}
	protected void Unload() {
		Model?.QueueFree();
		Model = null;
	}

	public override void _ExitTree() {
		base._ExitTree();
		Unload();
	}
	public override void _EnterTree() {
		base._EnterTree();
		Load();

	}

	public override SingleWeaponSaveData Save() {
		return new SingleWeaponSaveData(this);
	}



	[Serializable]
	public class SingleWeaponSaveData(SingleWeapon weapon) : SceneSaveData<Weapon>(weapon) {
		public string? CostumePath { get; set; } = weapon.Costume?.ResourcePath;


		public override SingleWeapon? Load() {
			if (base.Load() is not SingleWeapon weapon) return null;

			if (CostumePath is not null) {
				WeaponCostume? costume = ResourceLoader.Load<WeaponCostume>(CostumePath);
				weapon.SetCostume(costume);
			}

			return weapon;
		}
	}
}