namespace LandlessSkies.Core;

using System.Linq;
using Godot;

[Tool]
[GlobalClass]
public abstract partial class SingleWeapon : Weapon {
	private Handedness _handedness = Handedness.Right;


	public override bool IsLoaded {
		get => WeaponModel is WeaponModel model && model.IsLoaded;
		set => WeaponModel?.AsILoadable().SetLoaded(value);
	}




	[Export] public override WeaponData Data {
		get => _data;
		protected set {
			if (_data is not null)
				return;
			_data = value;

			if (this.IsInitializationSetterCall())
				return;

			if (Costume is null) {
				SetCostume(_data?.BaseCostume);
			}

			Name = $"{nameof(Weapon)} - {_data?.DisplayName}";
		}
	}
	private WeaponData _data = null!;

	[ExportGroup("Costume")]
	[Export] private WeaponModel? WeaponModel;

	[Export] public override WeaponCostume? Costume {
		get => WeaponModel?.Costume;
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

	public override ICustomizable[] Children => [.. base.Children.Append(WeaponModel!)];

	protected SingleWeapon() : base() {}
	public SingleWeapon(WeaponData? data, WeaponCostume? costume) : base() {
		SetData(data, costume);
	}


	protected void SetData(WeaponData? data, WeaponCostume? costume = null) {
		_data = data!;
		SetCostume(costume ?? _data?.BaseCostume);

		Name = _data is null ? "Weapon" : $"Weapon - {_data.DisplayName}";
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


	public ISaveData<Weapon> Save() {
		return new SingleWeaponSaveData(Data, Costume);
	}


	public override void _Parented() {
		base._Parented();
		Callable.From(() => WeaponModel?.SafeReparentAndSetOwner(this)).CallDeferred();
	}



	protected class SingleWeaponSaveData(WeaponData data, WeaponCostume? costume) : ISaveData<Weapon> {
		public Weapon Load() {
			return data.Instantiate(costume);
		}
	}
}