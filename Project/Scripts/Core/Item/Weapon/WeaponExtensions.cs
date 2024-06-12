namespace LandlessSkies.Core;

using System.Collections.Generic;
using System.Collections.Immutable;

public static class WeaponExtensions {
	public static Weapon? Bundle(this IEnumerable<SingleWeapon> weapons) =>
		weapons.ToImmutableArray() switch {
			[]                                      => null,
			[SingleWeapon single]                   => single,
			// [Weapon first, Weapon second]           => new DualWeapon(first, second),
			[.. ImmutableArray<SingleWeapon> many]  => new MultiWeapon([.. many]),
		};
}