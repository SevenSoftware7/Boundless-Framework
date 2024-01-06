using System.Collections.Generic;
using System.Collections.Immutable;

namespace LandlessSkies.Core;

public static class WeaponExtensions {
	public static Weapon? Bundle(this IEnumerable<Weapon> weapons) =>
		weapons.ToImmutableArray() switch {
			[] => null,
			[Weapon single]                   => single,
			// [Weapon first, Weapon second]     => new DualWeapon(first, second),
			[.. ImmutableArray<Weapon> many]  => new MultiWeapon([.. many]),
		};
}