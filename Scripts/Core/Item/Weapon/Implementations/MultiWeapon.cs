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
	public IWeapon? CurrentWeapon {
		get => _currentIndex < _weapons.Count ? _weapons[(int)_currentIndex] : null;
		private set => SwitchTo(value);
	}

	[Injector]
	[Injectable]
	private WeaponHolsterState HolsterState;

	public override uint MaxStyle => (uint)(_weapons.Count != 0 ? _weapons.Count - 1 : 0);

	[ExportGroup("Current Weapon")]
	[Export]
	public override uint Style {
		get => _currentIndex;
		set => SwitchTo(value);
	}
	private uint _currentIndex;

	protected override IWeapon? Weapon => CurrentWeapon;


	private MultiWeapon() : base() { }
	public MultiWeapon(IEnumerable<IWeapon?> weapons) : this() {
		foreach (Node weaponNode in weapons.OfType<Node>()) {
			weaponNode.SetOwnerAndParent(this);
		}
	}
	public MultiWeapon(ImmutableArray<ISaveData<IWeapon>> weaponSaves) : this(weaponSaves.Select(save => save.Load())) { }



	private void UpdateCurrent() {
		if (_currentIndex >= _weapons.Count) {
			_currentIndex = 0;
		}

		this.PropagateInjection<WeaponHolsterState>();
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
			.Select(a => {
				if (a.Builder is Attack.Builder attack && attack.Weapon != currentWeapon) {
					a.BeforeExecute += () => SwitchTo(attack.Weapon);
					a.AfterExecute += () => SwitchTo(currentWeapon);
				}
				return a;
			})
			.Concat(base.GetAttacks(target));
	}


	WeaponHolsterState IInjectionInterceptor<WeaponHolsterState>.Intercept(Node child, WeaponHolsterState value) {
		return child == CurrentWeapon ? value : WeaponHolsterState.Holstered;
	}


	public override List<ICustomization> GetCustomizations() => base.GetCustomizations();
	public override List<ICustomizable> GetSubCustomizables() {
		List<ICustomizable> list = base.GetSubCustomizables();
		return [.. list.Concat(_weapons)];
	}
	public override ISaveData<IWeapon> Save() {
		return new MultiWeaponSaveData([.. _weapons]);
	}


	protected override void UpdateWeapons() {
		_weapons = [.. GetChildren().OfType<IWeapon>()];
		UpdateCurrent();
	}


	[Serializable]
	public class MultiWeaponSaveData(IEnumerable<IWeapon> weapons) : ISaveData<MultiWeapon> {
		private readonly ISaveData<IWeapon>[] WeaponSaves = weapons
			.Select(w => w.Save())
			.ToArray();


		public MultiWeapon Load() {
			return new MultiWeapon([.. WeaponSaves]);
		}
	}
}