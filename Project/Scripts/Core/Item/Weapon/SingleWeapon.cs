namespace LandlessSkies.Core;

using System;
using Godot;

[Tool]
[GlobalClass]
public abstract partial class SingleWeapon : Weapon, IUIObject {

	[Export] public string DisplayName { get; private set; } = string.Empty;
	public Texture2D? DisplayPortrait => Costume?.DisplayPortrait;
	public override IUIObject UIObject => this;



	public override bool IsLoaded {
		get => _isLoaded;
		set => this.BackingFieldLoadUnload(ref _isLoaded, value);
	}
	private bool _isLoaded = false;


	[ExportGroup("Costume")]
	[Export] private Model? WeaponModel;
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

	[ExportGroup("Dependencies")]
	[Export] public override Skeleton3D? Skeleton {
		get => _skeleton;
		protected set {
			_skeleton = value;

			if (this.IsInitializationSetterCall())
				return;

			if (WeaponModel is ISkeletonAdaptable mSkeleton) mSkeleton.SetParentSkeleton(value);
		}
	}
	private Skeleton3D? _skeleton;

	[Export] public override Handedness Handedness {
		get => _handedness;
		protected set {
			_handedness = value;

			if (this.IsInitializationSetterCall())
				return;

			if (WeaponModel is IHandAdaptable mHanded) mHanded.SetHandedness(value);
		}
	}
	private Handedness _handedness = Handedness.Right;



	public override int Style {
		get => _style;
		set => _style = value % (StyleCount + 1);
	}
	private int _style;


	[Signal] public delegate void CostumeChangedEventHandler(WeaponCostume? newCostume, WeaponCostume? oldCostume);


	protected SingleWeapon() : base() { }
	public SingleWeapon(WeaponCostume? costume) : this() {
		SetCostume(costume);
	}



	public void SetCostume(WeaponCostume? newCostume) {
		WeaponCostume? oldCostume = _costume;
		if (newCostume == oldCostume)
			return;

		_costume = newCostume;

		AsILoadable().Reload();

		EmitSignal(SignalName.CostumeChanged, newCostume!, oldCostume!);
	}



	// private void OnCharacterLoadedUnloaded(bool isLoaded) {
	// 	Skeleton3D? result = isLoaded ? Skeleton : null;
	// 	WeaponModel?.Inject(result);
	// }

	public override void SetParentSkeleton(Skeleton3D? skeleton) {
		base.SetParentSkeleton(skeleton);
		if (skeleton is null) {
			AsILoadable().Unload();
		}
		else {
			AsILoadable().Load();
		}
	}

	protected override bool LoadBehaviour() {
		new LoadableUpdater<Model>(ref WeaponModel, () => Costume?.Instantiate())
			.BeforeLoad(m => {
				if (m is ISkeletonAdaptable mSkeleton) mSkeleton.SetParentSkeleton(Skeleton);
				if (m is IHandAdaptable mHanded) mHanded.SetHandedness(Handedness);
				m.SafeReparentEditor(this);
				m.AsIEnablable().EnableDisable(IsEnabled);
			})
			.Execute();

		_isLoaded = true;

		return true;
	}
	protected override void UnloadBehaviour() {
		new LoadableDestructor<Model>(ref WeaponModel)
			.AfterUnload(w => w.QueueFree())
			.Execute();

		_isLoaded = false;
	}

	protected override void EnableBehaviour() {
		base.EnableBehaviour();
		WeaponModel?.AsIEnablable().Enable();
	}
	protected override void DisableBehaviour() {
		base.DisableBehaviour();
		WeaponModel?.AsIEnablable().Disable();
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


	public override void _Parented() {
		base._Parented();
		Callable.From(() => WeaponModel?.SafeReparentAndSetOwner(this)).CallDeferred();
	}
}