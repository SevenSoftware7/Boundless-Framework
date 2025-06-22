namespace SevenDev.Boundless.Modding;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Godot;
using SevenDev.Boundless.Utility;


public class Mod : IDisposable {
	public static readonly Type ModInterfaceType = typeof(IModInterface);

	internal static readonly Dictionary<ModManifest, Mod> LoadedModsDictionary = [];
	public static IEnumerable<Mod> LoadedMods => LoadedModsDictionary.Values;
	private static readonly DirectoryPath ModAssetsPath = new("res://ModAssets/");

	private bool _disposed = false;
	private bool _started = false;

	public ModManifest MetaData { get; init; }

	public IEnumerable<(Assembly, AssemblyLoadContext)> Assemblies { get; private set; }
	public IEnumerable<IModInterface> ModInterfaces { get; private set; }

	public IEnumerable<PckFile> AssetPacks { get; private set; }


	internal static Mod? Load(ModManifest metaData) {
		ref Mod? mod = ref CollectionsMarshal.GetValueRefOrAddDefault(LoadedModsDictionary, metaData, out bool exists);
		if (exists) return mod;

		if (Engine.IsEditorHint()) {
			throw new InvalidOperationException("Cannot load mods in the editor.");
		}
		return mod = new Mod(metaData);
	}
	private Mod(ModManifest metaData) {

		MetaData = metaData;

		Assemblies = [..
			metaData.AssemblyPaths
				.Select(metaData.Path.Directory.Combine)
				.Select(LoadAssembly)
				.OfType<(Assembly, AssemblyLoadContext)>()
		];

		ModInterfaces = [..
			Assemblies.Select(pair => pair.Item1)
				.SelectMany(assembly => assembly.GetTypes())
				.SelectMany(type => type.GetMethods())
				.Where(method => method.IsStatic && method.ReturnType == ModInterfaceType && method.GetParameters().Length == 0)
				.Select(method => method.Invoke(null, null))
				.OfType<IModInterface>()
		];

		AssetPacks = metaData.AssetPaths
			.Select(metaData.Path.Directory.Combine)
			.Select(PckFile.Load);
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

		LoadedModsDictionary.Remove(MetaData);


		Stop();

		foreach ((_, AssemblyLoadContext context) in Assemblies) {
			context.Unload();
		}

		Assemblies = null!;
		ModInterfaces = null!;

		AssetPacks = null!;
	}

	public void Unload() => Dispose();


	private static (Assembly, AssemblyLoadContext)? LoadAssembly(FilePath assemblyPath) {
		if (string.IsNullOrEmpty(assemblyPath.Path)) return null;

		byte[] file = Godot.FileAccess.GetFileAsBytes(assemblyPath);
		if (file.Length == 0) {
			GD.PrintErr($"[Boundless.Modding]: {Godot.FileAccess.GetOpenError()}");
			return null;
		}
		MemoryStream stream = new(file);

		AssemblyLoadContext assemblyLoadContext = new GodotResAssemblyLoadContext(assemblyPath.Directory);
		return (assemblyLoadContext.LoadFromStream(stream), assemblyLoadContext);
	}

	public void Start() {
		if (_disposed) return;
		if (_started) return;
		_started = true;


		GD.Print($"[Boundless.Modding]: Starting mod {MetaData.Name}");

		DirectoryPath installPath = ModAssetsPath.CombineDirectory(MetaData.Name);
		foreach (PckFile assetPack in AssetPacks) {
			assetPack.Install(installPath);
		}
		foreach (IModInterface modInterface in ModInterfaces) {
			modInterface.Start();
		}

		GD.Print($"[Boundless.Modding]: Started mod {MetaData.Name}");
	}
	public void Stop() {
		if (!_started) return;
		_started = false;


		GD.Print($"[Boundless.Modding]: Stopping mod {MetaData.Name}");

		foreach (IModInterface modInterface in ModInterfaces) {
			modInterface.Stop();
		}
		foreach (PckFile assetPack in AssetPacks) {
			assetPack.Uninstall();
		}

		GD.Print($"[Boundless.Modding]: Stopped mod {MetaData.Name}");
	}
}