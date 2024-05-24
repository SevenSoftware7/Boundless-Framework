namespace SevenDev.Utility;

using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using KGySoft.CoreLibraries;


public static class IEnumerableExtensions {
	public static bool ValueEquals<T>(this IEnumerable<T> enum1, IEnumerable<T> enum2) {
		return enum1.Count() == enum2.Count() && enum1.All(enum2.Contains);
	}

	public static void RemoveRange<[MustBeVariant] T>(this Array<T> collection, IEnumerable<T> toRemove) {
		foreach (T item in toRemove) {
			collection.Remove(item);
		}
	}
}