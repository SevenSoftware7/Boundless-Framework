using Godot;

namespace LandlessSkies.Core;

[Tool]
public abstract partial class SingleWeapon<T> : SingleWeapon where T : WeaponData {
	public override WeaponData WeaponData {
		get => Data;
		protected set => Data = (value as T)!;
	}

	public T Data {
		get => _data;
		protected set {
			if (_data is not null)
				return;

			if (this.IsInitializationSetterCall()) {
				_data = value;
				return;
			}

			SetData(_data);
		}
	}
	private T _data = null!;

	public override IUIObject UIObject => Data;
	public override IWeapon.Size WeaponSize => Data.Size;
	public override IWeapon.Type WeaponType => Data.Type;



	protected SingleWeapon() : base() {}
	public SingleWeapon(T? data, WeaponCostume? costume) : this() {
		SetData(data ?? ResourceManager.GetRegisteredWeapon<T>(), costume);
	}



	protected void SetData(T? data, WeaponCostume? costume = null) {
		_data = data!;
		SetCostume(costume ?? _data?.BaseCostume);

		Name = _data is null ? "Weapon" : $"Weapon - {_data.DisplayName}";
	}

	public override void _Ready() {
		if (Data is null) {
			SetData(ResourceManager.GetRegisteredWeapon<T>());
		}
		base._Ready();
	}

	public override ISaveData<SingleWeapon> SingleWeaponSave() {
		return new SingleWeaponSaveData(Data, Costume);
	}
	protected class SingleWeaponSaveData(T data, WeaponCostume? costume) : ISaveData<SingleWeapon> {
		public SingleWeapon Load() {
			return data.Instantiate(costume);
		}
	}
}