namespace SevenDev.Boundless.Modding;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using SevenDev.Boundless.Utility;

internal static class ModLoader {
	private static readonly DirectoryPath ModDirectoryName = new("mods");
	private static readonly DirectoryPath UserModsPath = ModDirectoryName with { Protocol = "user" };
	private static readonly DirectoryPath InternalModsPath = ModDirectoryName with { Protocol = "res" };


	private static ModMetaData? ParseModConfig(FilePath modFilePath) {

		byte[]? file = FileAccess.GetFileAsBytes(modFilePath);
		if (file.Length == 0) {
			GD.PrintErr(FileAccess.GetOpenError());
			return null;
		}
		string modConfig = System.Text.Encoding.UTF8.GetString(file);

		ModMetaData metaData = ModMetaData.FromYaml(modConfig);
		metaData.Path = modFilePath;

		return metaData;
	}

	private static IEnumerable<ModMetaData> ReadModFolder(DirectoryPath modFolder) {
		IEnumerable<string> modFilesNames = DirAccess.GetFilesAt(modFolder);

		return modFilesNames
			.Where(file => file.EndsWith("mod.yaml", StringComparison.OrdinalIgnoreCase))
			.Select(file => ParseModConfig(modFolder.CombineFile(file)))
			.Where(metaData => metaData is not null)
			.Select(metaData => metaData!);
	}


	public static IEnumerable<ModMetaData> ReadUserModFolder(string folderName) =>
		ReadModFolder(UserModsPath.CombineDirectory(folderName));

	public static IEnumerable<ModMetaData> ReadInternalModFolder(string folderName) =>
		ReadModFolder(InternalModsPath.CombineDirectory(folderName));


	public static IEnumerable<ModMetaData> ReadAllUserMods() {
		IEnumerable<string> modFolderNames = DirAccess.GetDirectoriesAt(UserModsPath);
		return modFolderNames.SelectMany(ReadUserModFolder);
	}

	public static IEnumerable<ModMetaData> ReadAllInternalMods() {
		IEnumerable<string> modFolderNames = DirAccess.GetDirectoriesAt(InternalModsPath);
		return modFolderNames.SelectMany(ReadInternalModFolder);
	}
}