namespace LandlessSkies.Core;

using Godot;
using SevenDev.Boundless.Utility;

public static class ModLoader {
	private static readonly DirectoryPath ModDirectoryName = new("mods");
	private static readonly DirectoryPath UserModsPath = ModDirectoryName with { Protocol = "user" };
	private static readonly DirectoryPath InternalModsPath = ModDirectoryName with { Protocol = "res" };


	private static ModMetaData? ParseModConfig(DirectoryPath modDirectory) {
		FilePath modConfigPath = modDirectory.CombineFile("mod.config.yml");

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
		DirectoryPath modDirectory = UserModsPath.CombineDirectory(modName);
		return ParseModConfig(modDirectory);
	}

	public static ModMetaData? ReadInternalMod(string modName) {
		DirectoryPath modDirectory = InternalModsPath.CombineDirectory(modName);
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