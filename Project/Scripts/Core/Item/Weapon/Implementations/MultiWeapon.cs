namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Serialization;
using Godot;
using Godot.Collections;
using SevenDev.Utility;

[Tool]
[GlobalClass]
public sealed partial class MultiWeapon : Weapon {
	[Export] private Array<Weapon?> Weapons {
		get => [.. _weapons];
		set {
			// If nothing changed
			if (_weapons.SequenceEqual(value)) return;

			// If the items were only moved
			if (_weapons.Count == value.Count && _weapons.Intersect(value).Count() == value.Count) {
				_weapons = [.. value];

				UpdateCurrent();
				RearrangeHierarchy();
				return;
			}

			int minLength = Math.Min(value.Count, _weapons.Count);
			for (int i = 0; i < Math.Max(value.Count, _weapons.Count); i++) {
				switch (i) {
				// If an item was modified
				case int index when index < minLength && _weapons[index] != value[index]:
					SetWeapon(index, value[index]);
					break;

				// If an item was added
				case int index when index >= minLength && index < value.Count:
					AddWeapon(value[index]);
					break;

				// If an item was removed
				case int index when index >= minLength && index >= value.Count:
					RemoveWeapon(index);
					break;
				}
			}

			UpdateCurrent();
		}
	}
	private List<Weapon?> _weapons = [];

	public override int StyleCount => Mathf.Max(_weapons.Count, 1);

	[ExportGroup("Current Weapon")]
	[Export] public override int Style {
		get => _currentIndex;
		set => SwitchTo(value);
	}
	private int _currentIndex;



	public Weapon? CurrentWeapon {
		get => IndexInBounds(_currentIndex) ? _weapons[_currentIndex] : null;
		private set {
			if (value is not null) {
				_currentIndex = _weapons.IndexOf(value);
			}
		}
	}


	public override string DisplayName => CurrentWeapon?.DisplayName ?? string.Empty;
	public override Texture2D? DisplayPortrait => CurrentWeapon?.DisplayPortrait;


	public override IWeapon.Type WeaponType => CurrentWeapon?.WeaponType ?? 0;
	public override IWeapon.Usage WeaponUsage => CurrentWeapon?.WeaponUsage ?? 0;
	public override IWeapon.Size WeaponSize => CurrentWeapon?.WeaponSize ?? 0;


	private MultiWeapon() : base() { }
	public MultiWeapon(ReadOnlySpan<Weapon> weapons) : this() {
		Weapons = [.. weapons];
	}
	public MultiWeapon(ImmutableArray<ISaveData<Weapon>> weaponSaves) : this() {
		foreach (ISaveData<Weapon> data in weaponSaves) {
			AddWeapon(data.Load());
		}
	}

	public override List<ICustomizable> GetSubCustomizables() {
		List<ICustomizable> list = base.GetSubCustomizables();
		return [.. list.Concat(_weapons)];
	}


	private bool IndexInBounds(int index) => index < _weapons.Count && index >= 0;
	private void UpdateCurrent() {
		if (! IndexInBounds(_currentIndex)) {
			_currentIndex = 0;
		}
		_weapons.ForEach((w) => w?.Disable());
		CurrentWeapon?.Enable();
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


	public void AddWeapon(Weapon? weapon) {
		_weapons.Add(null!);

		SetWeapon(_weapons.Count - 1, weapon);
	}

	public void SetWeapon(int index, Weapon? weapon) {
		if (! IndexInBounds(index)) return;

		if (weapon is null) return;

		weapon.SafeReparent(this);
		weapon.Inject(Handedness);
		weapon.Inject(Skeleton);

		_weapons[index] = weapon;

		UpdateCurrent();
		RearrangeHierarchy();
	}


	public void RemoveWeapon(int index) {
		if (! IndexInBounds(index)) return;

		_weapons.RemoveAt(index);

		UpdateCurrent();
		RearrangeHierarchy();
	}

	private void RearrangeHierarchy() {
		if (Engine.IsEditorHint()) {
			List<Weapon> weapons = Weapons.OfType<Weapon>().ToList();
			foreach (Weapon weapon in weapons) {
				weapon.GetParent().MoveChild(weapon, weapons.IndexOf(weapon));
			}
		}
	}


	public override IEnumerable<AttackActionInfo> GetAttacks(Entity target) {
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


	public override void _Notification(int what) {
		base._Notification(what);
		switch ((ulong)what) {
		case NotificationChildOrderChanged:
			Callable.From(() => Weapons = [.. GetChildren().OfType<Weapon>()]).CallDeferred(); // Modifying the Hierarchy is locked because of the Notification
			break;
		}
	}


	public override MultiWeaponSaveData Save() {
		return new MultiWeaponSaveData([.. _weapons]);
	}



	[Serializable]
	public class MultiWeaponSaveData(IEnumerable<Weapon> weapons) : ISaveData<Weapon> {
		private readonly ISaveData<Weapon>[] WeaponSaves = weapons
			.Select(w => w.Save())
			.ToArray();

		Weapon? ISaveData<Weapon>.Load() => Load();
		public MultiWeapon Load() {
			return new MultiWeapon([.. WeaponSaves]);
		}
	}
}