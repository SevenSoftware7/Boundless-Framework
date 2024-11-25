using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Godot;
using YamlDotNet.Serialization;
using System.Linq;
using System;

namespace LandlessSkies.Core;

public static class ModLoader {
	private static readonly string ModDirectoryName = "mods";
	private static readonly string UserModsPath = $"user://{ModDirectoryName}";
	private static readonly string InternalModsPath = $"res://{ModDirectoryName}";

	public static ModMetaData? ReadUserMod(string modName) {
		string modDirectory = Path.Combine(UserModsPath, modName);
		return ParseModConfig(modDirectory);
	}

	public static ModMetaData? ReadInternalMod(string modName) {
		string modDirectory = Path.Combine(InternalModsPath, modName);
		return ParseModConfig(modDirectory);
	}

	public static Mod? LoadUserMod(string modName) {
		ModMetaData? metaData = ReadUserMod(modName);
		if (metaData is null) return null;
		return Mod.Load(metaData);
	}

	public static Mod? LoadInternalMod(string modName) {
		ModMetaData? metaData = ReadInternalMod(modName);
		if (metaData is null) return null;
		return Mod.Load(metaData);
	}


	private static ModMetaData? ParseModConfig(string modDirectory) {
		string modConfigPath = Path.Combine(modDirectory, "mod.config.yml");

		byte[]? file = Godot.FileAccess.GetFileAsBytes(modConfigPath);
		if (file.Length == 0) {
			GD.PrintErr(Godot.FileAccess.GetOpenError());
			return null;
		}
		string modConfig = System.Text.Encoding.UTF8.GetString(file);

		ModMetaData metaData = ModMetaData.FromYaml(modConfig);
		metaData.Directory = modDirectory;

		return metaData;
	}
}