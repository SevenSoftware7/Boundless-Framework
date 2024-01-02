using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;


namespace LandlessSkies.Core;


[Tool]
[GlobalClass]
public sealed partial class WeaponInventory : Weapon, IUIObject {
	private Entity? _entity;
	private List<Weapon> _weapons = [];
	private int _currentIndex = 0;



	[Export] private Array<Weapon> Weapons {
		get => [.. _weapons];
		set {
			if (value is null) {
				_weapons = [];
				return;
			}

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


	[ExportGroup("Dependencies")]
	[Export] public Entity? Entity {
		get => _entity;
		set => Inject(value);
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
			if ( CurrentWeapon is not Weapon weapon ) {
				return IWeapon.Handedness.Right;
			}

			return weapon.WeaponHandedness;
		}
		set {
			if ( CurrentWeapon is not null ) {
				CurrentWeapon.WeaponHandedness = value;
			}
		}
	}

	public string DisplayName => "Inventory";
	public Texture2D? DisplayPortrait => CurrentWeapon?.UIObject.DisplayPortrait;

	public override IUIObject UIObject => this;
	public override ICustomizable[] Children => [.. base.Children.Concat( _weapons.Cast<ICustomizable>() )];



	private WeaponInventory() : base() {}



	private bool IndexInBounds(int index) => index < _weapons.Count && index >= 0;
	private void UpdateCurrent() {
		if ( ! IndexInBounds(_currentIndex) ) {
			_currentIndex = 0;
		}
		_weapons.ForEach((w) => w?.Disable());
		CurrentWeapon?.Enable();
	}

	public void SwitchTo(Weapon? weapon) {
		// if ( this.IsEditorGetSetter() ) return;
		if ( weapon is null ) return;

		int index = _weapons?.IndexOf(weapon) ?? -1;

		SwitchTo(index);
	}

	public void SwitchTo(int index) {
		// if ( this.IsEditorGetSetter() ) return;
		if ( ! IndexInBounds(index) ) return;

		_currentIndex = index;

		UpdateCurrent();
	}


	public void AddWeapon(WeaponData? data, WeaponCostume? costume = null) {
		// if ( this.IsEditorGetSetter() ) return;

		_weapons.Add(null!);
#if TOOLS
		_weaponDatas.Add(null!);
#endif

		SetWeapon(_weapons.Count, data, costume);
	}

	public void SetWeapon(int index, WeaponData? data, WeaponCostume? costume = null) {
		// if ( this.IsEditorGetSetter() ) return;
		if ( ! IndexInBounds(index) ) return;

		Weapon? weapon = _weapons[index];
		if ( data is not null && data == weapon?.Data ) return;

		LoadableExtensions.UpdateLoadable(ref weapon!)
			.WithConstructor(() => data?.Instantiate(this, costume))
			.BeforeLoad(() => weapon?.Inject(Entity))
			.Execute();

		_weapons[index] = weapon!;
#if TOOLS
		_weaponDatas[index] = data!;
#endif
		UpdateCurrent();
	}

	public void RemoveWeapon(int index) {
		// if ( this.IsEditorGetSetter() ) return;
		if ( ! IndexInBounds(index) ) return;

		Weapon? weapon = _weapons[index];
		LoadableExtensions.DestroyLoadable(ref weapon)
			.Execute();

		_weapons.RemoveAt(index);
#if TOOLS
		_weaponDatas.RemoveAt(index);
#endif
		UpdateCurrent();
	}

	public void SetCostume(int index, WeaponCostume? costume) {
		if ( ! IndexInBounds(index) ) return;

		_weapons[index]?.SetCostume(costume);
	}

	public override void SetCostume(WeaponCostume? costume) {
		CurrentWeapon?.SetCostume(costume);
	}


	public override IEnumerable<AttackAction.IAttackInfo> GetAttacks(Entity target) =>
		_weapons
			.SelectMany( (w) => w.GetAttacks(target) )
			.Select( a => {
				Weapon? currentWeapon = CurrentWeapon;
				if (a.Weapon != currentWeapon) {
					a.BeforeExecute += () => SwitchTo(a.Weapon);
					a.AfterExecute += () => SwitchTo(currentWeapon);
				}
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


	public override void _ValidateProperty(Dictionary property) {
		base._ValidateProperty(property);
		
		switch (property["name"].AsStringName()) {
			case nameof(Costume):
			case nameof(Data):
				property["usage"] = (int)(property["usage"].As<PropertyUsageFlags>() | PropertyUsageFlags.ReadOnly);
				break;
		}
	}
}