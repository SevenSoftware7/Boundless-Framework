
using System;
using System.Collections.Generic;

namespace LandlessSkies.Core;

public interface IWeapon : IDataContainer<WeaponData>, ICostumable<WeaponCostume> {
	Type WeaponType { get; }
	Handedness WeaponHandedness { get; }



	IEnumerable<AttackAction.Info> GetAttacks(Entity target);



	[Flags]
	public enum Type : byte {
		Sparring = 1 << 0,
		OneHanded = 1 << 1,
		TwoHanded = 1 << 2,
		Polearm = 1 << 3,
		Shield = 1 << 4,
		Dagger = 1 << 5,
	};

	[Flags]
	public enum Handedness : byte {
		Left = 1 << 0,
		Right = 1 << 1,
	}
}