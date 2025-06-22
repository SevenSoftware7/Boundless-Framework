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
	internal static readonly Dictionary<ModMetaData, Mod> LoadedModsDictionary = [];
	public static IEnumerable<Mod> LoadedMods => LoadedModsDictionary.Values;

	private static readonly DirectoryPath ModAssetsPath = new("res://ModAssets/");
	private bool _disposed = false;
	private bool _started = false;

	public ModMetaData MetaData { get; init; }

	public (Assembly, AssemblyLoadContext)[] Assemblies { get; private set; }
	public PckFile[] AssetPacks { get; private set; }

	internal static Mod? Load(ModMetaData metaData) {
		ref Mod? mod = ref CollectionsMarshal.GetValueRefOrAddDefault(LoadedModsDictionary, metaData, out bool exists);
		if (exists) return mod;

		return mod = new Mod(metaData);
	}
	private Mod(ModMetaData metaData) {
		if (Engine.IsEditorHint()) {
			throw new InvalidOperationException("Cannot load mods in the editor.");
		}

		MetaData = metaData;

		Assemblies = [..
			metaData.AssemblyPaths
				.Select(metaData.Path.Directory.Combine)
				.Select(LoadAssembly)
				.OfType<(Assembly, AssemblyLoadContext)>()
		];

		AssetPacks = [..
			metaData.AssetPaths
				.Select(metaData.Path.Directory.Combine)
				.Select(PckFile.Load)
		];
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

		AssetPacks = null!;
	}

	public void Unload() => Dispose();


	private static (Assembly, AssemblyLoadContext)? LoadAssembly(FilePath assemblyPath) {
		if (string.IsNullOrEmpty(assemblyPath.Path)) return null;

		byte[] file = Godot.FileAccess.GetFileAsBytes(assemblyPath);
		if (file.Length == 0) {
			GD.PrintErr(Godot.FileAccess.GetOpenError());
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

		GD.Print($"[Boundless.Modding] : Starting mod {MetaData.Name}");

		DirectoryPath installPath = ModAssetsPath.CombineDirectory(MetaData.Name);
		foreach (PckFile assetPack in AssetPacks) {
			assetPack.Install(installPath);
		}

		foreach ((Assembly assembly, _) in Assemblies) {
			assembly.GetTypes().SelectMany(type => type.GetMethods())
				.Where(method => method.Name == "Main" && method.GetParameters().Length == 0)
				.ToList().ForEach(method => method.Invoke(null, null));
		}

		GD.Print($"[Boundless.Modding] : Started mod {MetaData.Name}");
	}
	public void Stop() {
		if (!_started) return;
		_started = false;

		GD.Print($"[Boundless.Modding] : Stopping mod {MetaData.Name}");

		foreach ((Assembly assembly, _) in Assemblies) {
			assembly.GetTypes().SelectMany(type => type.GetMethods())
				.Where(method => method.Name == "Stop" && method.GetParameters().Length == 0)
				.ToList().ForEach(method => method.Invoke(null, null));
		}
		foreach (PckFile assetPack in AssetPacks) {
			assetPack.Uninstall();
		}

		GD.Print($"[Boundless.Modding] : Stopped mod {MetaData.Name}");
	}
}