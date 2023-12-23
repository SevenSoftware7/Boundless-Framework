using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;


namespace LandlessSkies.Core;


[Tool]
[GlobalClass]
public sealed partial class WeaponInventory : Weapon {

	private Entity? _entity;
	private List<Weapon> _weapons = [];
	private int _currentIndex = 0;
	
	

	[Export] private Array<Weapon> Weapons {
		get => [.. _weapons];
		set {
			_weapons = [.. value];
#if TOOLS
			if ( this.IsEditorGetSetter() ) return;
			ResetData();
#endif
		}
	}

	[ExportGroup("Current Weapon")]
	[Export] private int CurrentIndex {
		get => _currentIndex;
		set {
			if ( this.IsEditorGetSetter() ) {
				_currentIndex = value;
				return;
			}

			SwitchTo(value);
		}
	}

	public Weapon? CurrentWeapon {
		get => IndexInBounds(CurrentIndex) ? _weapons[CurrentIndex] : null;
		private set {
			if (value is not null) {
				int index = _weapons.IndexOf(value);
				CurrentIndex = index;
			}
		}
	}

	public override WeaponData Data {
		get => CurrentWeapon?.Data!;
		protected set {
			if (CurrentWeapon is not null) {
				SetWeapon(_currentIndex, value);
			}
		}
	}
	public override WeaponCostume? Costume {
		get => CurrentWeapon?.Costume;
		set {
			if (CurrentWeapon is Weapon currWeapon) {
				currWeapon.Costume = value;
			}
		}
	}

	public override IWeapon.Handedness WeaponHandedness {
		get {
			if ( ! IndexInBounds(CurrentIndex) || _weapons[CurrentIndex] is not Weapon weapon ) return IWeapon.Handedness.Right;

			return weapon.WeaponHandedness;
		}
		set {
			if (CurrentWeapon is not null) {
				CurrentWeapon.WeaponHandedness = value;
			}
		}
	}


	[ExportGroup("Dependencies")]
	[Export] public Entity? Entity {
		get => _entity;
		set => Inject(value);
	}



	private WeaponInventory() : base() {}



	private bool IndexInBounds(int index) {
		return index < _weapons.Count;
	}

	public void SwitchTo(Weapon? weapon) {
		if ( this.IsEditorGetSetter() ) return;
		if ( _weapons is null || _weapons.Count == 0 ) {
			_currentIndex = 0;
			return;
		}

		if (weapon is null) return;
		int index = _weapons.IndexOf(weapon);
		if (index == -1) return;

		SwitchTo(index);
	}

	public void SwitchTo(int index) {
		if ( this.IsEditorGetSetter() ) return;
		if ( _weapons is null || _weapons.Count == 0 ) {
			_currentIndex = 0;
			return;
		}

		int maxCount = _weapons.Count - 1;
		_currentIndex = index > maxCount ? maxCount : index;

		for (int i = 0; i < _weapons.Count; i++) {
			_weapons[i]?.Disable();
		}

		_weapons[_currentIndex]?.Enable();
	}


	public void AddWeapon(WeaponData? data, WeaponCostume? costume = null) {
		if ( this.IsEditorGetSetter() ) return;
		int index = _weapons.Count;

		_weapons.Add(null!);
#if TOOLS
		_weaponDatas.Add(null!);
#endif

		SetWeapon(index, data, costume);
	}

	public void SetWeapon(int index, WeaponData? data, WeaponCostume? costume = null) {
		if ( this.IsEditorGetSetter() ) return;
		if (index >= _weapons.Count) return;

		Weapon? weapon = _weapons[index];
		if ( data is not null && weapon?.Data == data ) return;

		LoadableExtensions.UpdateLoadable(ref weapon!)
			.WithConstructor(() => {
				Weapon? weapon = data?.Instantiate(this, costume);
				_weapons[index] = weapon!;
				return weapon;
			})
			.BeforeLoad(() => weapon?.Inject(Entity))
			.Execute();

#if TOOLS
		_weaponDatas[index] = data!;
#endif
	}

	public void RemoveWeapon(int index) {
		if ( this.IsEditorGetSetter() ) return;
		if (index >= _weapons.Count) return;

		Weapon? weapon = _weapons[index];
		LoadableExtensions.DestroyLoadable(ref weapon)
			.Execute();

		_weapons.RemoveAt(index);
#if TOOLS
		_weaponDatas.RemoveAt(index);
#endif
	}

	public void SetCostume(int index, WeaponCostume? costume) {
		if (index >= _weapons.Count) return;

		_weapons[index]?.SetCostume(costume);
	}

	public override void SetCostume(WeaponCostume? costume) =>
		CurrentWeapon?.SetCostume(costume);

	public override IEnumerable<AttackAction.Info> GetAttacks(Entity target) =>
		_weapons
			.SelectMany( (w) => w.GetAttacks(target) )
			.Select( a => {
				Weapon? currentWeapon = CurrentWeapon;
				a.BeforeExecute += () => SwitchTo(a.Weapon);
				a.AfterExecute += () => SwitchTo(currentWeapon);
				return a;
			});

	public override void Inject(Entity? entity) {
		_entity = entity;
		_weapons?.ForEach(w => w?.Inject(entity));
	}


	public override void HandleInput(Player.InputInfo inputInfo) {
		base.HandleInput(inputInfo);
		CurrentWeapon?.HandleInput(inputInfo);
	}


	public override void Enable() =>
		CurrentWeapon?.Enable();

	public override void Disable() =>
		_weapons?.ForEach(w => w?.Disable());

	public override void Destroy() =>
		_weapons?.ForEach(w => w?.Destroy());

	public override void ReloadModel(bool forceLoad = false) =>
		_weapons?.ForEach(w => w?.ReloadModel(forceLoad));


	protected override bool LoadModelImmediate() {
		_weapons?.ForEach(w => w?.LoadModel());
		return true;
	}

	protected override bool UnloadModelImmediate() {
		_weapons?.ForEach(w => w?.UnloadModel());
		return true;
	}

}