namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using SevenDev.Boundless.Persistence;

public interface IWeapon : IUIObject {
	WeaponKind Kind { get; }
	WeaponUsage Usage { get; }
	WeaponSize Size { get; }

	abstract StyleState Style { get; }
	virtual StyleState MaxStyle => 0;

	IEnumerable<Action.Wrapper> GetAttacks(Entity target);

	[Flags]
	public enum WeaponSize : byte {
		OneHanded = 1 << 0,
		TwoHanded = 1 << 1,
	};

	[Flags]
	public enum WeaponKind : byte {
		Sword = 1 << 0,
		Sparring = 1 << 1,
		Polearm = 1 << 2,
		Shield = 1 << 3,
		Dagger = 1 << 4,
	};

	[Flags]
	public enum WeaponUsage : byte {
		Slash = 1 << 0,
		Thrust = 1 << 1,
		Strike = 1 << 2,
	};
}