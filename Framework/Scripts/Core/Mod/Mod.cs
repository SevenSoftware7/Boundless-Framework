namespace Seven.Boundless.Modding;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Godot;
using Seven.Boundless.Utility;


public class Mod : IDisposable {
	public static readonly Type ModInterfaceType = typeof(IModInterface);

	internal static readonly Dictionary<ModInfo, Mod> LoadedModsDictionary = [];
	public static IEnumerable<Mod> LoadedMods => LoadedModsDictionary.Values;
	private static readonly DirectoryPath ModAssetsPath = new("res://ModAssets/");

	private bool _disposed = false;
	private bool _started = false;

	public ModInfo Info { get; init; }

	public IEnumerable<(Assembly, AssemblyLoadContext)> Assemblies { get; private set; } = [];
	public IEnumerable<IModInterface> ModInterfaces { get; private set; } = [];

	public IEnumerable<PckFile> AssetPacks { get; private set; } = [];


	internal static Mod? Load(ModInfo metaData) {
		ref Mod? mod = ref CollectionsMarshal.GetValueRefOrAddDefault(LoadedModsDictionary, metaData, out bool exists);
		if (exists) return mod;

		if (Engine.IsEditorHint()) {
			throw new InvalidOperationException("Cannot load mods in the editor.");
		}
		return mod = new Mod(metaData);
	}
	private Mod(ModInfo info) {
		Info = info;
		using GodotFileStream zipStream = new(info.ZipPath, Godot.FileAccess.ModeFlags.Read);

		Assemblies = info.Manifest.AssemblyPaths
			.Select(path => LoadAssembly(zipStream, path))
			.OfType<(Assembly, AssemblyLoadContext)>()
			.ToArray();

		ModInterfaces = [..
			Assemblies.Select(pair => pair.Item1)
				.SelectMany(assembly => assembly.GetTypes())
				.SelectMany(type => type.GetMethods())
				.Where(method => method.IsStatic && method.ReturnType == ModInterfaceType && method.GetParameters().Length == 0)
				.Select(method => method.Invoke(null, null))
				.OfType<IModInterface>()
		];

		AssetPacks = info.Manifest.AssetPaths
			.Select(path => LoadAssetPack(zipStream, path))
			.OfType<PckFile>()
			.ToArray();
	}

	~Mod() {
		Dispose(false);
	}
	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}
	private void Dispose(bool disposing) {
		if (_disposed) return;
		_disposed = true;

		LoadedModsDictionary.Remove(Info);


		Stop();

		foreach ((_, AssemblyLoadContext context) in Assemblies) {
			context.Unload();
		}

		Assemblies = null!;
		ModInterfaces = null!;

		AssetPacks = null!;
	}

	public void Unload() => Dispose();


	private static (Assembly, AssemblyLoadContext)? LoadAssembly(Stream zipStream, FilePath assemblyPath) {
		assemblyPath = assemblyPath with { Directory = assemblyPath.Directory with { Protocol = string.Empty} };
		if (string.IsNullOrEmpty(assemblyPath.Path)) return null;

		using ZipArchive zipArchive = new(zipStream, ZipArchiveMode.Read, true);
		if (zipArchive.GetEntry(assemblyPath) is not ZipArchiveEntry entry) {
			GD.PrintErr($"[Boundless.Modding]: Assembly {assemblyPath} not found in mod");
			return null;
		}

		using Stream stream = entry.Open();
		using MemoryStream memoryStream = new();
		stream.CopyTo(memoryStream);
		memoryStream.Position = 0;

		ZipFileAssemblyLoadContext assemblyLoadContext = new(zipArchive, assemblyPath.Directory);
		return (assemblyLoadContext.LoadFromStream(memoryStream), assemblyLoadContext);
	}
	private static PckFile? LoadAssetPack(Stream zipStream, FilePath assetPackPath) {
		assetPackPath = assetPackPath with { Directory = assetPackPath.Directory with { Protocol = string.Empty} };
		if (string.IsNullOrEmpty(assetPackPath.Path)) return null;

		using ZipArchive zipArchive = new(zipStream, ZipArchiveMode.Read, true);
		if (zipArchive.GetEntry(assetPackPath) is not ZipArchiveEntry entry) {
			GD.PrintErr($"[Boundless.Modding]: Asset pack {assetPackPath} not found in mod");
			return null;
		}

		using Stream stream = entry.Open();
		using MemoryStream memoryStream = new();
		stream.CopyTo(memoryStream);
		memoryStream.Position = 0;

		return PckFile.Load(memoryStream);
	}

	public void Start() {
		if (_disposed) return;
		if (_started) return;
		_started = true;


		GD.Print($"[Boundless.Modding]: Starting mod {Info.Manifest.Name}");

		DirectoryPath installPath = ModAssetsPath.CombineDirectory(Info.Manifest.Name);
		foreach (PckFile assetPack in AssetPacks) {
			assetPack.Install(installPath);
		}
		foreach (IModInterface modInterface in ModInterfaces) {
			modInterface.Start();
		}

		GD.Print($"[Boundless.Modding]: Started mod {Info.Manifest.Name}");
	}
	public void Stop() {
		if (!_started) return;
		_started = false;


		GD.Print($"[Boundless.Modding]: Stopping mod {Info.Manifest.Name}");

		foreach (IModInterface modInterface in ModInterfaces) {
			modInterface.Stop();
		}
		foreach (PckFile assetPack in AssetPacks) {
			assetPack.Uninstall();
		}

		GD.Print($"[Boundless.Modding]: Stopped mod {Info.Manifest.Name}");
	}
}