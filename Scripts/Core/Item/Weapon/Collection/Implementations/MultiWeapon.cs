namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Godot;
using SevenDev.Boundless.Utility;
using SevenDev.Boundless.Injection;
using SevenDev.Boundless.Persistence;

[Tool]
[GlobalClass]
public sealed partial class MultiWeapon : CompositeWeapon, IInjectionInterceptor<WeaponHolsterState>, IInjectionBlocker<StyleState>, IInjectionInterceptor<StyleState>, IPersistent<MultiWeapon> {
	public IInjectionNode InjectionNode { get; }

	private List<IWeapon> _weapons = [];
	private uint _currentIndex;
	[Export] private uint CurrentIndex {
		get => _currentIndex;
		set {
			uint newValue = value % ((uint)MaxStyle + 1);
			if (_currentIndex == newValue) return;

			_currentIndex = newValue;

			this.PropagateInjection<WeaponHolsterState>();
		}
	}

	public override IWeapon? CurrentWeapon => _currentIndex < _weapons.Count ? _weapons[(int)_currentIndex] : null;

	[Injector]
	[Injectable]
	private WeaponHolsterState HolsterState;

	WeaponHolsterState IInjectionInterceptor<WeaponHolsterState>.Intercept(IInjectionNode child, WeaponHolsterState value) {
		return child.UnderlyingObject == CurrentWeapon ? value : WeaponHolsterState.Holstered;
	}


	public override StyleState Style => _currentIndex;
	public override StyleState MaxStyle => _weapons.Count != 0 ? _weapons.Count - 1 : 0;

	StyleState IInjectionInterceptor<StyleState>.Intercept(IInjectionNode child, StyleState value) {
		if (value == _currentIndex && child.UnderlyingObject is IWeapon weapon && _weapons.Contains(weapon)) {
			return weapon.Style + 1;
		}

		// Reset style on Weapon to be equipped
		// TODO: if the Entity is a player, check for the preference setting
		// to get the corresponding switch-to-weapon behaviour.
		return StyleState.Primary;
	}
	bool IInjectionBlocker<StyleState>.ShouldBlock(IInjectionNode child, StyleState value) => child.UnderlyingObject is IWeapon weapon && _weapons.IndexOf(weapon) != value;


	private MultiWeapon() : base() {
		InjectionNode = new GodotNodeInjectionNode(this);
	}
	public MultiWeapon(IEnumerable<IWeapon?> weapons) : this() {
		foreach (Node weaponNode in weapons.OfType<Node>()) {
			weaponNode.SetOwnerAndParent(this);
		}
	}
	public MultiWeapon(ImmutableArray<IPersistenceData<IWeapon>> weaponSaves, IItemDataProvider registry) : this(weaponSaves.Select(save => save.Load(registry))) { }


	protected override void _RefreshWeapons() {
		IEnumerable<IWeapon> nonNodeWeapons = _weapons.Where(w => w is not Node);
		_weapons = [.. GetChildren().OfType<IWeapon>().Concat(nonNodeWeapons)];

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

		_listLock = true;
		weaponNode.SetOwnerAndParent(this);
		_weapons.Add(weapon);
		_listLock = false;
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

		_listLock = true;
		if (weaponNode.GetParent() == this) {
			RemoveChild(weaponNode);
			_weapons.Remove(weapon);
		}
		_listLock = false;
	}
	public void RemoveWeapons(IEnumerable<IWeapon> weapons) {
		foreach (IWeapon weapon in weapons) {
			RemoveWeapon(weapon);
		}
	}


	public void SwitchTo(IWeapon? weapon) {
		if (weapon is null) return;
		CurrentIndex = (uint)_weapons.IndexOf(weapon);
	}
	[Injectable] public void SwitchTo(StyleState style) => CurrentIndex = (uint)style;

	public override IEnumerable<IWeapon> GetWeapons() => _weapons;
	public override IEnumerable<EntityAction.Wrapper> GetAttacks(Entity target) {
		IWeapon? currentWeapon = CurrentWeapon;
		return base.GetAttacks(target)
			.Select(a => {
				if (a.Builder is Attack.Builder attack && attack.Weapon != currentWeapon) {
					a.BeforeExecute += () => SwitchTo(attack.Weapon);
					a.AfterExecute += () => SwitchTo(currentWeapon);
				}
				return a;
			});
	}


	public override IEnumerable<IUIObject> GetSubObjects() {
		IEnumerable<IUIObject>? subObjects = base.GetSubObjects();

		subObjects = subObjects.Concat(_weapons);

		return subObjects;
	}


	public override IPersistenceData<MultiWeapon> Save() => new MultiWeaponSaveData(this);


	[Serializable]
	public class MultiWeaponSaveData(MultiWeapon multiWeapon) : PersistenceData<MultiWeapon>(multiWeapon) {
		private readonly ImmutableArray<IPersistenceData<IWeapon>> WeaponSaves = [.. multiWeapon._weapons
			.OfType<IPersistent<IWeapon>>()
			.Select(w => w.Save()) ];

		protected override MultiWeapon Instantiate(IItemDataProvider registry) => new(WeaponSaves, registry);
	}
}