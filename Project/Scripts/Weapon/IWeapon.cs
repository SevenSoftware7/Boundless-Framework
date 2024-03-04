
using System;
using System.Collections.Generic;

namespace LandlessSkies.Core;

public interface IWeapon : IInputReader, ICustomizable, IDataContainer<WeaponData>, ICostumable<WeaponCostume> {
	Type WeaponType { get; }
	Size WeaponSize { get; }
	Handedness WeaponHandedness { get; }



	IEnumerable<AttackAction.IInfo> GetAttacks(Entity target);


	[Flags]
	public enum Type : byte {
		Sword = 1 << 0,
		Sparring = 1 << 1,
		Polearm = 1 << 2,
		Shield = 1 << 3,
		Dagger = 1 << 4,
	};

	[Flags]
	public enum Usage : byte {
		Slashing = 1 << 0,
		Thrusting = 1 << 1,
		Bludgeoning = 1 << 2,
	};

	[Flags]
	public enum Size : byte {
		OneHanded = 1 << 0,
		TwoHanded = 1 << 1,
	};
}