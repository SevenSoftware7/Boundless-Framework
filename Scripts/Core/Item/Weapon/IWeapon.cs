namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using SevenDev.Boundless.Persistence;

public interface IWeapon : IUIObject {
	public WeaponKind Kind { get; }
	public WeaponUsage Usage { get; }
	public WeaponSize Size { get; }

	public abstract StyleState Style { get; }
	public virtual StyleState MaxStyle => 0;

	public IEnumerable<EntityAction.Wrapper> GetAttacks(Entity target);

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