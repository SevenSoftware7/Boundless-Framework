namespace LandlessSkies.Core;

using System;

[Flags]
public enum WeaponSize : byte {
	OneHanded = 1 << 0,
	TwoHanded = 1 << 1,
};