namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Godot;
using SevenDev.Boundless.Utility;
using SevenDev.Boundless.Injection;

[Tool]
[GlobalClass]
public sealed partial class MultiWeapon : WeaponCollection, IInjectionInterceptor<WeaponHolsterState> {
	private List<IWeapon> _weapons = [];

	public override IWeapon? CurrentWeapon => _currentIndex < _weapons.Count ? _weapons[(int)_currentIndex] : null;

	[Injector]
	[Injectable]
	private WeaponHolsterState HolsterState;

	public override uint MaxStyle => (uint)(_weapons.Count != 0 ? _weapons.Count - 1 : 0);

	[Export]
	public override uint Style {
		get => _currentIndex;
		set => SwitchTo(value);
	}


	private uint _currentIndex;


	private MultiWeapon() : base() { }
	public MultiWeapon(IEnumerable<IWeapon?> weapons) : this() {
		foreach (Node weaponNode in weapons.OfType<Node>()) {
			weaponNode.SetOwnerAndParent(this);
		}
	}
	public MultiWeapon(ImmutableArray<IPersistenceData<IWeapon>> weaponSaves) : this(weaponSaves.Select(save => save.Load())) { }



	private void UpdateCurrent() {
		if (_currentIndex >= _weapons.Count) {
			_currentIndex = 0;
		}

		this.PropagateInjection<WeaponHolsterState>();
	}

	public void AddWeapon(IWeapon weapon) {
		if (weapon is not Node weaponNode) {
			_weapons.Add(weapon);
			return;
		}

		_lockBackingList = true;
		weaponNode.SetOwnerAndParent(this);
		_weapons.Add(weapon);
		_lockBackingList = false;
	}
	public void AddWeapons(IEnumerable<IWeapon> weapons) {
		foreach (IWeapon weapon in weapons) {
			AddWeapon(weapon);
		}
	}

	public void RemoveWeapon(IWeapon weapon) {
		if (weapon is not Node weaponNode) {
			_weapons.Remove(weapon);
			return;
		}

		_lockBackingList = true;
		if (weaponNode.GetParent() == this) {
			RemoveChild(weaponNode);
			_weapons.Remove(weapon);
		}
		_lockBackingList = false;
	}
	public void RemoveWeapons(IEnumerable<IWeapon> weapons) {
		foreach (IWeapon weapon in weapons) {
			RemoveWeapon(weapon);
		}
	}


	public void SwitchTo(IWeapon? weapon) {
		if (weapon is null) return;
		SwitchTo((uint)_weapons.IndexOf(weapon));
	}

	public void SwitchTo(uint index) {
		uint newIndex = index % (MaxStyle + 1);
		if (newIndex == _currentIndex && CurrentWeapon is IWeapon currentWeapon) {
			currentWeapon.Style++;
			return;
		}

		_currentIndex = newIndex;

		// Reset style on Weapon to be equipped
		// TODO: if the Entity is a player, check for the preference setting
		// to get the corresponding switch-to-weapon behaviour.
		if (CurrentWeapon is IWeapon newWeapon) {
			newWeapon.Style = 0;
		}

		UpdateCurrent();
	}


	public override IEnumerable<Action.Wrapper> GetAttacks(Entity target) {
		IWeapon? currentWeapon = CurrentWeapon;
		return _weapons
			.SelectMany((w) => w?.GetAttacks(target) ?? [])
			.Concat(base.GetAttacks(target))
			.Select(a => {
				if (a.Builder is Attack.Builder attack && attack.Weapon != currentWeapon) {
					a.BeforeExecute += () => SwitchTo(attack.Weapon);
					a.AfterExecute += () => SwitchTo(currentWeapon);
				}
				return a;
			});
	}


	WeaponHolsterState IInjectionInterceptor<WeaponHolsterState>.Intercept(Node child, WeaponHolsterState value) {
		return child == CurrentWeapon ? value : WeaponHolsterState.Holstered;
	}


	// public override Dictionary<StringName, ICustomization> GetCustomizations() => base.GetCustomizations();
	public override IEnumerable<IUIObject> GetSubObjects() {
		IEnumerable<IUIObject>? subObjects = base.GetSubObjects();

		subObjects = subObjects.Concat(_weapons);

		return subObjects;
	}



	protected override void UpdateWeapons() {
		IEnumerable<IWeapon> nonNodeWeapons = _weapons.Where(w => w is not Node);
		_weapons = [.. GetChildren().OfType<IWeapon>().Concat(nonNodeWeapons)];
		UpdateCurrent();
	}

	public override IPersistenceData<MultiWeapon> Save() => new MultiWeaponSaveData(this);


	[Serializable]
	public class MultiWeaponSaveData(MultiWeapon multiWeapon) : PersistenceData<MultiWeapon>(multiWeapon) {
		private readonly ImmutableArray<IPersistenceData<IWeapon>> WeaponSaves = [.. multiWeapon._weapons
			.OfType<IPersistent<IWeapon>>()
			.Select(w => w.Save()) ];

		protected override MultiWeapon Instantiate() => new(WeaponSaves);
	}
}