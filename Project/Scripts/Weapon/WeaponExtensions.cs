using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace LandlessSkies.Core;

public static class WeaponExtensions {
	public static Weapon? Bundle(this IEnumerable<SingleWeapon> weapons) =>
		weapons.ToImmutableArray() switch {
			[] => null,
			[Weapon single]                         => single,
			// [Weapon first, Weapon second]           => new DualWeapon(first, second),
			[.. ImmutableArray<SingleWeapon> many]  => new MultiWeapon([.. many]),
		};

	public static Weapon? Create(this IEnumerable<KeyValuePair<WeaponData, WeaponCostume?>> info) =>
		info.ToImmutableArray() switch {
			[] => null,
			[KeyValuePair<WeaponData, WeaponCostume?> single]                                                => single.Key.Instantiate(single.Value),
			// [KeyValuePair<WeaponData, WeaponCostume> first, KeyValuePair<WeaponData, WeaponCostume> second]  => new DualWeapon(first, second),
			[.. ImmutableArray<KeyValuePair<WeaponData, WeaponCostume?>> many]                               => new MultiWeapon(many),
		};
}