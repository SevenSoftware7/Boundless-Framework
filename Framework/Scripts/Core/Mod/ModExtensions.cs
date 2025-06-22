namespace SevenDev.Boundless.Modding;

using System.Collections.Generic;


public static class ModExtensions {
	public static void Load(this IEnumerable<ModManifest> mods) {
		foreach (ModManifest modManifest in mods) {
			Mod.Load(modManifest);
		}
	}
}