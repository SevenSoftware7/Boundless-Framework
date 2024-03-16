namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Godot;
using Godot.Collections;

[Tool]
[GlobalClass]
public sealed partial class MultiWeapon : Weapon, IUIObject {
	public override bool IsLoaded {
		get => CurrentWeapon is Weapon currentWeapon && currentWeapon.IsLoaded;
		set => CurrentWeapon?.AsILoadable().SetLoaded(value);
	}


	[Export] private Array<SingleWeapon?> Weapons {
		get => [.. _weapons];
		set {
			_weapons = [.. value];

			if (this.IsInitializationSetterCall())
				return;

			_weapons.ForEach(w => w?.SafeReparentEditor(this));
		}
	}
	private List<SingleWeapon?> _weapons = [];

	public override int Style {
		get => _currentIndex;
		set {
			if (value == _currentIndex && CurrentWeapon is SingleWeapon currentWeapon) {
				currentWeapon.Style++;
				return;
			}

			SwitchTo(value);
		}
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



	public SingleWeapon? CurrentWeapon {
		get => IndexInBounds(CurrentIndex) ? _weapons[CurrentIndex] : null;
		private set {
			if (value is not null) {
				CurrentIndex = _weapons.IndexOf(value);
			}
		}
	}

	public override WeaponData Data {
		get => CurrentWeapon?.Data!;
		protected set {
			if (this.IsInitializationSetterCall())
				return;
			SetWeapon(_currentIndex, value);
		}
	}
	public override WeaponCostume? Costume {
		get => CurrentWeapon?.Costume;
		set { if (CurrentWeapon is SingleWeapon currentWeapon) currentWeapon.Costume = value; }
	}


	public override Handedness Handedness {
		get {
			if (CurrentWeapon is not SingleWeapon weapon) {
				return Handedness.Right;
			}

			return weapon.Handedness;
		}
		set {
			if (CurrentWeapon is not null) {
				CurrentWeapon.Handedness = value;
			}
		}
	}

	public string DisplayName => "Inventory";
	public Texture2D? DisplayPortrait => CurrentWeapon?.UIObject.DisplayPortrait;

	public override IUIObject UIObject => this;
	public override ICustomizable[] Children => [.. base.Children.Concat(_weapons.Cast<ICustomizable>())];



	private MultiWeapon() : base() { }
	public MultiWeapon(IEnumerable<SingleWeapon> weapons) : this() {
		Weapons = [.. weapons];
	}
	public MultiWeapon(ImmutableArray<KeyValuePair<WeaponData, WeaponCostume?>> weaponsInfo) : this() {
		foreach (KeyValuePair<WeaponData, WeaponCostume?> data in weaponsInfo) {
			AddWeapon(data.Key, data.Value);
		}
	}



	private bool IndexInBounds(int index) => index < _weapons.Count && index >= 0;
	private void UpdateCurrent() {
		if (!IndexInBounds(_currentIndex)) {
			_currentIndex = 0;
		}
		_weapons.ForEach((w) => w?.Disable());
		CurrentWeapon?.Enable();
	}

	public void SwitchTo(SingleWeapon? weapon) =>
		SwitchTo(_weapons.IndexOf(weapon));

	public void SwitchTo(int index) {
		_currentIndex = index % Math.Max(_weapons.Count, 1);

		UpdateCurrent();
	}


	public void AddWeapon(WeaponData? data, WeaponCostume? costume = null) {
		_weapons.Add(null!);

		SetWeapon(_weapons.Count - 1, data, costume);
	}

	public void SetWeapon(int index, WeaponData? data, WeaponCostume? costume = null) {
		if (! IndexInBounds(index))
			return;

		SingleWeapon? weapon = _weapons[index];
		if (data is not null && data == weapon?.Data)
			return;

		new LoadableUpdater<SingleWeapon>(ref weapon, () => data?.Instantiate(costume))
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

	public void SetCostume(int index, WeaponCostume? costume) {
		if (!IndexInBounds(index))
			return;

		_weapons[index]?.SetCostume(costume);
	}

	public override void SetCostume(WeaponCostume? costume) {
		CurrentWeapon?.SetCostume(costume);
	}


	public override IEnumerable<AttackAction.IInfo> GetAttacks(Entity target) {
		SingleWeapon? currentWeapon = CurrentWeapon;
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

		this.SafeReparentEditor(entity);

		_weapons.ForEach(w => w?.Inject(entity));
	}

	public override void HandleInput(Player.InputInfo inputInfo) {
		base.HandleInput(inputInfo);
		CurrentWeapon?.HandleInput(inputInfo);
	}

	public override void Enable() => CurrentWeapon?.Enable();
	public override void Disable() => _weapons.ForEach(w => w?.Disable());
	public override void Destroy() => _weapons.ForEach(w => w?.Destroy());

	protected override bool LoadModelBehaviour() => CurrentWeapon?.AsILoadable().LoadModel() ?? false;
	protected override void UnloadModelBehaviour() => CurrentWeapon?.AsILoadable().UnloadModel();
	public void ReloadModel(bool forceLoad = false) => CurrentWeapon?.AsILoadable().ReloadModel(forceLoad);


	public ISaveData<Weapon> Save() {
		return new MultiWeaponSaveData([.. _weapons]);
	}


	public class MultiWeaponSaveData(IEnumerable<Weapon> weapons) : ISaveData<Weapon> {
		private readonly KeyValuePair<WeaponData, WeaponCostume?>[] Info = weapons
			.Select(w => new KeyValuePair<WeaponData, WeaponCostume?>(w.Data, w.Costume))
			.ToArray();

		public Weapon Load() {
			return new MultiWeapon([.. Info]);
		}
	}
}