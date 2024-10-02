namespace LandlessSkies.Core;

using System;

[Flags]
public enum WeaponType : byte {
	Sword = 1 << 0,
	Sparring = 1 << 1,
	Polearm = 1 << 2,
	Shield = 1 << 3,
	Dagger = 1 << 4,
};