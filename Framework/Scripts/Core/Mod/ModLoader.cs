namespace SevenDev.Boundless.Modding;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using SevenDev.Boundless.Utility;

public static class ModLoader {
	private static readonly DirectoryPath ModDirectoryName = new("mods");
	private static readonly DirectoryPath UserModsPath = ModDirectoryName with { Protocol = "user" };
	private static readonly DirectoryPath InternalModsPath = ModDirectoryName with { Protocol = "res" };


	private static ModManifest? ParseModConfig(FilePath modFilePath) {

		byte[]? file = FileAccess.GetFileAsBytes(modFilePath);
		if (file.Length == 0) {
			GD.PrintErr($"[Boundless.Modding]: {FileAccess.GetOpenError()}");
			return null;
		}
		string modConfig = System.Text.Encoding.UTF8.GetString(file);

		ModManifest metaData = ModManifest.FromYaml(modConfig);
		metaData.Path = modFilePath;

		return metaData;
	}

	private static IEnumerable<ModManifest> ReadModFolder(DirectoryPath modFolder) {
		IEnumerable<string> modFilesNames = DirAccess.GetFilesAt(modFolder);

		return modFilesNames
			.Where(file => file.EndsWith("mod.yaml", StringComparison.OrdinalIgnoreCase))
			.Select(file => ParseModConfig(modFolder.CombineFile(file)))
			.Where(metaData => metaData is not null)
			.Select(metaData => metaData!);
	}


	public static IEnumerable<ModManifest> ReadUserModFolder(string folderName) =>
		ReadModFolder(UserModsPath.CombineDirectory(folderName));
	public static IEnumerable<Mod> LoadUserModFolder(string folderName) =>
		ReadUserModFolder(folderName)
			.Select(metaData => Mod.Load(metaData))
			.Where(mod => mod is not null)
			.Select(mod => mod!);

	public static IEnumerable<ModManifest> ReadInternalModFolder(string folderName) =>
		ReadModFolder(InternalModsPath.CombineDirectory(folderName));
	public static IEnumerable<Mod> LoadInternalModFolder(string folderName) =>
		ReadInternalModFolder(folderName)
			.Select(metaData => Mod.Load(metaData))
			.Where(mod => mod is not null)
			.Select(mod => mod!);


	public static IEnumerable<ModManifest> ReadAllUserMods() {
		IEnumerable<string> modFolderNames = DirAccess.GetDirectoriesAt(UserModsPath);
		return modFolderNames.SelectMany(ReadUserModFolder);
	}
	public static IEnumerable<Mod> LoadAllUserMods() =>
		ReadAllUserMods()
			.Select(metaData => Mod.Load(metaData))
			.Where(mod => mod is not null)
			.Select(mod => mod!);

	public static IEnumerable<ModManifest> ReadAllInternalMods() {
		IEnumerable<string> modFolderNames = DirAccess.GetDirectoriesAt(InternalModsPath);
		return modFolderNames.SelectMany(ReadInternalModFolder);
	}
	public static IEnumerable<Mod> LoadAllInternalMods() =>
		ReadAllInternalMods()
			.Select(metaData => Mod.Load(metaData))
			.Where(mod => mod is not null)
			.Select(mod => mod!);
}