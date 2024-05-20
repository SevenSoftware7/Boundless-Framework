namespace SevenDev.Utility;

using System.Collections.Generic;
using System.Linq;

public static class IEnumerableExtensions {
	public static bool ValueEquals<T>(this IEnumerable<T> enum1, IEnumerable<T> enum2) {
		return enum1.Count() == enum2.Count() && enum1.All(enum2.Contains);
	}
}