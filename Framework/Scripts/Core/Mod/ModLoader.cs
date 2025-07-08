namespace SevenDev.Boundless.Modding;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Godot;
using SevenDev.Boundless.Utility;

public static class ModLoader {
	private static readonly DirectoryPath ModDirectoryName = new("mods");
	private static readonly DirectoryPath UserModsPath = ModDirectoryName with { Protocol = "user" };
	private static readonly DirectoryPath InternalModsPath = ModDirectoryName with { Protocol = "res" };

	private static ModInfo? LoadModManifest(Stream manifestStream, FilePath modFile, FilePath manifestPath) {
		string manifestContents;
		using (StreamReader reader = new(manifestStream)) {
			manifestContents = reader.ReadToEnd();
		}
		ModManifest? loadedManifest = ModManifest.FromYaml(manifestContents);
		if (loadedManifest is null) return null;

		return new ModInfo(
			modFile,
			manifestPath,
			loadedManifest
		);
	}
	private static ModInfo[] ReadModFile(Stream zipStream, FilePath modFile) {
		using ZipArchive zipReader = new(zipStream, ZipArchiveMode.Read, true);

		return zipReader.Entries
			.Where(entry => entry.Name.EndsWith("mod.yaml", StringComparison.OrdinalIgnoreCase))
			.Select(entry => {
				using Stream entryStream = entry.Open();
				return LoadModManifest(entryStream, modFile, new(entry.FullName));
			})
			.Where(manifest => manifest.HasValue)
			.Select(manifest => manifest!.Value)
			.ToArray();
	}
	private static ModInfo[] ReadModFile(FilePath modFile) {
		modFile = modFile with { Extension = "zip" };
		using GodotFileStream zipStream = new(modFile, Godot.FileAccess.ModeFlags.Read);

		return ReadModFile(zipStream, modFile);
	}


	public static IEnumerable<ModInfo> ReadUserModFile(string fileName) =>
		ReadModFile(UserModsPath.CombineFile(fileName));
	public static IEnumerable<Mod> LoadUserModFile(string fileName) =>
		ReadUserModFile(fileName)
			.Select(Mod.Load)
			.Where(mod => mod is not null)
			.Select(mod => mod!)
			.ToArray();

	public static IEnumerable<ModInfo> ReadInternalModFile(string fileName) =>
		ReadModFile(InternalModsPath.CombineFile(fileName));
	public static IEnumerable<Mod> LoadInternalModFile(string fileName) =>
		ReadInternalModFile(fileName)
			.Select(Mod.Load)
			.Where(mod => mod is not null)
			.Select(mod => mod!)
			.ToArray();


	public static IEnumerable<ModInfo> ReadAllUserMods() {
		IEnumerable<string> modFileNames = DirAccess.GetFilesAt(UserModsPath)
			.Where(file => file.EndsWith("zip", StringComparison.OrdinalIgnoreCase));
		return modFileNames.SelectMany(ReadUserModFile);
	}
	public static IEnumerable<Mod> LoadAllUserMods() =>
		ReadAllUserMods()
			.Select(Mod.Load)
			.Where(mod => mod is not null)
			.Select(mod => mod!)
			.ToArray();

	public static IEnumerable<ModInfo> ReadAllInternalMods() {
		IEnumerable<string> modFileNames = DirAccess.GetFilesAt(InternalModsPath)
			.Where(file => file.EndsWith("zip", StringComparison.OrdinalIgnoreCase));
		return modFileNames.SelectMany(ReadInternalModFile);
	}
	public static IEnumerable<Mod> LoadAllInternalMods() =>
		ReadAllInternalMods()
			.Select(Mod.Load)
			.Where(mod => mod is not null)
			.Select(mod => mod!)
			.ToArray();
}