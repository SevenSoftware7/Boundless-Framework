namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public abstract partial class SingleWeapon : Weapon {
	public override bool IsLoaded {
		get => WeaponModel?.IsLoaded ?? false;
		set => WeaponModel?.AsILoadable().SetLoaded(value);
	}


	[ExportGroup("Costume")]
	[Export] private Model? WeaponModel;

	[Export] public WeaponCostume? Costume {
		get => WeaponModel?.Costume as WeaponCostume;
		set {
			if (this.IsInitializationSetterCall())
				return;
			SetCostume(value);
		}
	}

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



	public void SetCostume(WeaponCostume? costume) {
		WeaponCostume? oldCostume = Costume;
		if (costume == oldCostume)
			return;

		new LoadableUpdater<Model>(ref WeaponModel, () => costume?.Instantiate())
			.BeforeLoad(m => {
				if (m is ISkeletonAdaptable mSkeleton) mSkeleton.SetParentSkeleton(Skeleton);
				if (m is IHandAdaptable mHanded) mHanded.SetHandedness(Handedness);
				m.SafeReparentEditor(this);
				m.AsIEnablable().EnableDisable(IsEnabled);
			})
			.Execute();

		EmitSignal(SignalName.CostumeChanged, costume!, oldCostume!);
	}



	// private void OnCharacterLoadedUnloaded(bool isLoaded) {
	// 	Skeleton3D? result = isLoaded ? Skeleton : null;
	// 	WeaponModel?.Inject(result);
	// }



	protected override bool LoadBehaviour() {
		// WeaponModel?.Inject(Skeleton);
		return WeaponModel?.AsILoadable().Load() ?? false;
	}
	protected override void UnloadBehaviour() {
		// WeaponModel?.Inject(null as Skeleton3D);
		WeaponModel?.AsILoadable().Unload();
	}

	protected override void EnableBehaviour() {
		base.EnableBehaviour();
		WeaponModel?.AsIEnablable().Enable();
	}
	protected override void DisableBehaviour() {
		base.DisableBehaviour();
		WeaponModel?.AsIEnablable().Disable();
	}



	public override void _Parented() {
		base._Parented();
		Callable.From(() => WeaponModel?.SafeReparentAndSetOwner(this)).CallDeferred();
	}

}