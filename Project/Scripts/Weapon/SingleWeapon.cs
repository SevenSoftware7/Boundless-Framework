namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public abstract partial class SingleWeapon : Weapon {
	public override bool IsLoaded {
		get => WeaponModel is Model model && model.IsLoaded;
		set => WeaponModel?.AsILoadable().SetLoaded(value);
	}


	[Export] public abstract WeaponData WeaponData { get; protected set; }

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
	[Export] public Entity? Entity {
		get => _entity;
		private set {
			if (this.IsInitializationSetterCall()) {
				_entity = value;
				return;
			}
			Inject(value);
		}
	}
	private Entity? _entity;

	protected virtual int StyleMax { get; } = 0;

	public override int Style {
		get => _style;
		set => _style = value % (StyleMax + 1);
	}
	private int _style;

	public override Handedness Handedness {
		get => _handedness;
		set {
			_handedness = value;
			WeaponModel?.Inject(value);
		}
	}
	private Handedness _handedness = Handedness.Right;


	[Signal] public delegate void CostumeChangedEventHandler(WeaponCostume? newCostume, WeaponCostume? oldCostume);



	protected SingleWeapon() : base() {}



	public void SetCostume(WeaponCostume? costume) {
		WeaponCostume? oldCostume = Costume;
		if (costume == oldCostume)
			return;

		new LoadableUpdater<Model>(ref WeaponModel, () => costume?.Instantiate())
			.BeforeLoad(m => {
				m.Inject(Entity?.Skeleton);
				m.Inject(Handedness);
				m.SafeReparentEditor(this);
			})
			.Execute();

		EmitSignal(SignalName.CostumeChanged, costume!, oldCostume!);
	}


	public override void Inject(Entity? entity) {
		if (entity == _entity)
			return;

		Callable callableLoadUnloadEvent = new(this, MethodName.OnCharacterLoadedUnloaded);
		_entity?.Disconnect(Entity.SignalName.CharacterLoadedUnloaded, callableLoadUnloadEvent);

		_entity = entity;

		WeaponModel?.Inject(entity?.Skeleton);
		entity?.Connect(Entity.SignalName.CharacterLoadedUnloaded, callableLoadUnloadEvent, (uint) ConnectFlags.Persist);
	}


	private void OnCharacterLoadedUnloaded(bool isLoaded) {
		Skeleton3D? result = isLoaded ? Entity?.Skeleton : null;
		WeaponModel?.Inject(result);
	}


	protected override bool LoadModelBehaviour() {
		WeaponModel?.Inject(Entity?.Skeleton);
		return WeaponModel?.AsILoadable().LoadModel() ?? false;
	}
	protected override void UnloadModelBehaviour() {
		WeaponModel?.Inject(null);
		WeaponModel?.AsILoadable().UnloadModel();
	}

	public override void Enable() {
		base.Enable();
		WeaponModel?.Enable();
	}
	public override void Disable() {
		base.Disable();
		WeaponModel?.Disable();
	}



	public override ISaveData<Weapon> Save() => (ISaveData<Weapon>) SingleWeaponSave();
	public abstract ISaveData<SingleWeapon> SingleWeaponSave();

	public override void _Parented() {
		base._Parented();
		Callable.From(() => WeaponModel?.SafeReparentAndSetOwner(this)).CallDeferred();
	}
}