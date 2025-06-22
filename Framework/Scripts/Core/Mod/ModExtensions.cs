namespace SevenDev.Boundless.Modding;

using System.Collections.Generic;


public static class ModExtensions {
	public static void Load(this IEnumerable<ModMetaData> mods) {
		foreach (ModMetaData mod in mods) {
			mod.Load();
		}
	}
}