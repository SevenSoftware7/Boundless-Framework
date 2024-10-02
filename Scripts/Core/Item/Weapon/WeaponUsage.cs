namespace LandlessSkies.Core;

using System;

[Flags]
public enum WeaponUsage : byte {
	Slash = 1 << 0,
	Thrust = 1 << 1,
	Strike = 1 << 2,
};