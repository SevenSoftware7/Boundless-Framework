namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Godot;
using Godot.Collections;

[Tool]
[GlobalClass]
public sealed partial class MultiWeapon : Weapon {
	public override bool IsLoaded {
		get => CurrentWeapon is Weapon currentWeapon && currentWeapon.IsLoaded;
		set => CurrentWeapon?.AsILoadable().SetLoaded(value);
	}


	public override WeaponData WeaponData {
		get => CurrentWeapon?.WeaponData!;
		protected set { }
	}
	[Export] private Array<Weapon?> Weapons {
		get => [.. _weapons];
		set {
			_weapons = [.. value];

			if (this.IsInitializationSetterCall())
				return;

			_weapons.ForEach(w => w?.SafeReparentEditor(this).Inject(Entity));
			UpdateCurrent();
		}
	}
	private List<Weapon?> _weapons = [];

	public override int StyleCount => Mathf.Max(_weapons.Count, 1);

	public override int Style {
		get => _currentIndex;
		set => SwitchTo(value);
	}

	[ExportGroup("Current Weapon")]
	[Export] private int CurrentIndex {
		get => _currentIndex;
		set {
			if (this.IsInitializationSetterCall()) {
				_currentIndex = value;
				return;
			}

			SwitchTo(value);
		}
	}
	private int _currentIndex = 0;


	[ExportGroup("Dependencies")]
	[Export] public Entity? Entity {
		get => _entity;
		set {
			if (this.IsInitializationSetterCall()) {
				_entity = value;
				return;
			}
			Inject(value);
		}
	}
	private Entity? _entity;



	public Weapon? CurrentWeapon {
		get => IndexInBounds(CurrentIndex) ? _weapons[CurrentIndex] : null;
		private set {
			if (value is not null) {
				CurrentIndex = _weapons.IndexOf(value);
			}
		}
	}


	public Texture2D? DisplayPortrait => CurrentWeapon?.UIObject.DisplayPortrait;

	public override IUIObject UIObject => null!;
	public override ICustomizable[] Children => [.. _weapons.Cast<ICustomizable>()];

	public override IWeapon.Type WeaponType => CurrentWeapon?.WeaponType ?? 0;
	public override IWeapon.Size WeaponSize => CurrentWeapon?.WeaponSize ?? 0;
	public override Handedness Handedness {
		get => CurrentWeapon?.Handedness ?? 0;
		set {
			if (CurrentWeapon is Weapon currentWeapon)
				currentWeapon.Handedness = value;
		}
	}

	private MultiWeapon() : base() { }
	public MultiWeapon(IEnumerable<Weapon> weapons) : this() {
		Weapons = [.. weapons];
	}
	public MultiWeapon(ImmutableArray<ISaveData<Weapon>> weaponSaves) : this() {
		foreach (ISaveData<Weapon> data in weaponSaves) {
			AddWeapon(data.Load());
		}
	}



	private bool IndexInBounds(int index) => index < _weapons.Count && index >= 0;
	private void UpdateCurrent() {
		if (!IndexInBounds(_currentIndex)) {
			_currentIndex = 0;
		}
		_weapons.ForEach((w) => w?.Disable());
		CurrentWeapon?.Enable();

		if (Engine.IsEditorHint()) {
			// Rearrange the weapons in the Node hierarchy
			List<Weapon> weapons = Weapons.OfType<Weapon>().ToList();
			foreach (Weapon weapon in weapons) {
				weapon.GetParent().MoveChild(weapon, weapons.IndexOf(weapon));
			}
		}
	}

	public void SwitchTo(Weapon? weapon) =>
		SwitchTo(_weapons.IndexOf(weapon));

	public void SwitchTo(int index) {
		int newIndex = index % StyleCount;
		if (newIndex == _currentIndex && CurrentWeapon is Weapon currentWeapon) {
			currentWeapon.Style++;
			return;
		}

		_currentIndex = newIndex;

		// TODO: if the Entity is a player, check for the corresponding preference setting
		if (CurrentWeapon is Weapon newWeapon) {
			newWeapon.Style = 0;
		}

		UpdateCurrent();
	}


	public void AddWeapon(Weapon weapon) {
		_weapons.Add(null!);

		SetWeapon(_weapons.Count - 1, weapon);
	}

	public void AddWeapon(WeaponData? data, WeaponCostume? costume = null) {
		_weapons.Add(null!);

		SetWeapon(_weapons.Count - 1, data, costume);
	}

	public void SetWeapon(int index, Weapon? weapon) {
		if (! IndexInBounds(index))
			return;

		if (weapon is null)
			return;

		new LoadableUpdater<Weapon>(ref weapon)
			.BeforeLoad(w => {
				w.Inject(Entity);
				w.SafeReparentEditor(this);
			})
			.Execute();

		_weapons[index] = weapon;
		UpdateCurrent();
	}

	public void SetWeapon(int index, WeaponData? data, WeaponCostume? costume = null) {
		if (! IndexInBounds(index))
			return;

		Weapon? weapon = _weapons[index];
		if (data is not null && data == weapon?.WeaponData)
			return;

		new LoadableUpdater<Weapon>(ref weapon, () => data?.Instantiate(costume))
			.BeforeLoad(w => {
				w.Inject(Entity);
				w.SafeReparentEditor(this);
			})
			.Execute();

		_weapons[index] = weapon;
		UpdateCurrent();
	}

	public void RemoveWeapon(int index) {
		if (!IndexInBounds(index))
			return;

		Weapon? weapon = _weapons[index];
		new LoadableDestructor<Weapon>(ref weapon)
			.Execute();

		_weapons.RemoveAt(index);
		UpdateCurrent();
	}


	public override IEnumerable<AttackAction.IInfo> GetAttacks(Entity target) {
		Weapon? currentWeapon = CurrentWeapon;
		return _weapons
			.SelectMany((w) => w?.GetAttacks(target) ?? [])
			.Select(a => {
				if (a.Weapon != currentWeapon) {
					a.BeforeExecute += () => SwitchTo(a.Weapon);
					a.AfterExecute += () => SwitchTo(currentWeapon);
				}
				return a;
			});
	}

	public override void Inject(Entity? entity) {
		_entity = entity;

		_weapons.ForEach(w => w?.Inject(entity));
	}

	public override void HandleStyleInput(Player.InputInfo inputInfo) {
		base.HandleStyleInput(inputInfo);
			if (inputInfo.InputDevice.IsActionJustPressed("switch_weapon_primary")) {
				Style = 0;
			} else if (inputInfo.InputDevice.IsActionJustPressed("switch_weapon_secondary")) {
				Style = 1;
			} else if (inputInfo.InputDevice.IsActionJustPressed("switch_weapon_ternary")) {
				Style = 2;
			}
	}
	public override void HandleInput(Player.InputInfo inputInfo) {
		base.HandleInput(inputInfo);
		CurrentWeapon?.HandleInput(inputInfo);
	}

	public override void Enable() {
		base.Enable();
		CurrentWeapon?.Enable();
	}
	public override void Disable() {
		_weapons.ForEach(w => w?.Disable());
		base.Disable();
	}
	public override void Destroy() {
		_weapons.ForEach(w => w?.Destroy());
		base.Destroy();
	}

	protected override bool LoadModelBehaviour() => CurrentWeapon?.AsILoadable().LoadModel() ?? false;
	protected override void UnloadModelBehaviour() => CurrentWeapon?.AsILoadable().UnloadModel();
	public void ReloadModel(bool forceLoad = false) => CurrentWeapon?.AsILoadable().ReloadModel(forceLoad);



	public ISaveData<MultiWeapon> SaveMulti() {
		return new MultiWeaponSaveData([.. _weapons]);
	}
	public override ISaveData<Weapon> Save() => (ISaveData<Weapon>)SaveMulti();


	public readonly struct MultiWeaponSaveData(IEnumerable<Weapon> weapons) : ISaveData<MultiWeapon> {
		private readonly ISaveData<Weapon>[] WeaponSaves = weapons
			.Select(w => w.Save())
			.ToArray();

		public MultiWeapon Load() {
			return new MultiWeapon([.. WeaponSaves]);
		}
	}
}