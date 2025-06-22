namespace SevenDev.Boundless;

using System.Collections.Generic;
using System.Collections.Immutable;

public static class WeaponExtensions {
	public static IWeapon? Bundle(this IEnumerable<Weapon> weapons) =>
		weapons.ToImmutableArray() switch {
			[] => null,
			[Weapon single] => single,
			[Weapon first, Weapon second] => new AkimboWeapon(first, second),
			[.. ImmutableArray<Weapon> many] => new MultiWeapon([.. many]),
		};
}