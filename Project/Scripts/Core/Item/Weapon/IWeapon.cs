namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;

public interface IWeapon : ICustomizable {
	Type WeaponType { get; }
	Usage WeaponUsage { get; }
	Size WeaponSize { get; }
	Handedness Handedness { get; }



	IEnumerable<AttackBuilder> GetAttacks(Entity target);


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
		Slash = 1 << 0,
		Thrust = 1 << 1,
		Strike = 1 << 2,
	};

	[Flags]
	public enum Size : byte {
		OneHanded = 1 << 0,
		TwoHanded = 1 << 1,
	};
}