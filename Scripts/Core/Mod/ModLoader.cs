using System.Collections.Generic;
using System.IO;
using Godot;

namespace LandlessSkies.Core;

public static class ModLoader {
	internal static readonly List<Mod> LoadedMods = [];

	private static readonly GodotPath ModDirectoryName = new("mods/");
	private static readonly GodotPath UserModsPath = new($"user://{ModDirectoryName}");
	private static readonly GodotPath InternalModsPath = new($"res://{ModDirectoryName}");


	private static ModMetaData? ParseModConfig(GodotPath modDirectory) {
		GodotPath modConfigPath = modDirectory.Combine("mod.config.yml");

		byte[]? file = Godot.FileAccess.GetFileAsBytes(modConfigPath);
		if (file.Length == 0) {
			GD.PrintErr(Godot.FileAccess.GetOpenError());
			return null;
		}
		string modConfig = System.Text.Encoding.UTF8.GetString(file);

		ModMetaData metaData = ModMetaData.FromYaml(modConfig);
		metaData.Path = modDirectory;

		return metaData;
	}


	public static ModMetaData? ReadUserMod(string modName) {
		GodotPath modDirectory = UserModsPath.Combine(modName + '/');
		return ParseModConfig(modDirectory);
	}

	public static ModMetaData? ReadInternalMod(string modName) {
		GodotPath modDirectory = InternalModsPath.Combine(modName + '/');
		return ParseModConfig(modDirectory);
	}

	public static Mod? LoadUserMod(string modName) {
		ModMetaData? metaData = ReadUserMod(modName);
		if (metaData is null) return null;
		return metaData.Load();
	}

	public static Mod? LoadInternalMod(string modName) {
		ModMetaData? metaData = ReadInternalMod(modName);
		if (metaData is null) return null;
		return metaData.Load();
	}
}