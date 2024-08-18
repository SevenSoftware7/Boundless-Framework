namespace LandlessSkies.Core;

using System.Collections.Generic;

public interface IWeapon : ICustomizable, ISaveable<IWeapon> {
	WeaponType Type { get; }
	WeaponUsage Usage { get; }
	WeaponSize Size { get; }

	abstract int Style { get; set; }
	virtual int StyleCount => 1;

	IEnumerable<Attack.Wrapper> GetAttacks(Entity target);
}