namespace Seven.Boundless.Modding;

using System.Collections.Generic;


public static class ModExtensions {
	public static void Load(this IEnumerable<ModInfo> mods) {
		foreach (ModInfo modInfo in mods) {
			Mod.Load(modInfo);
		}
	}
}