namespace LandlessSkies.Core;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Godot;

public record class Mod : IDisposable {
	private bool _disposed = false;

	public ModMetaData MetaData { get; init; }

	public (Assembly, AssemblyLoadContext)[] Assemblies { get; init; }
	public PckFile[] AssetPacks { get; init; }


	public Mod(ModMetaData metaData) {
		MetaData = metaData;

		if (Engine.IsEditorHint()) {
			throw new InvalidOperationException("Cannot load mods in the editor.");
		}

		Assemblies = metaData.AssemblyPaths
			.Select(path => Path.Combine(metaData.Directory, path))
			.Select(LoadAssembly)
			.OfType<(Assembly, AssemblyLoadContext)>()
			.ToArray();

		AssetPacks = metaData.AssetPaths
			.Select(path => Path.Combine(metaData.Directory, path))
			.Select(PckLoader.Load)
			.ToArray();


		string installPath = $"res://ModAssets/{metaData.Name}/";
		foreach (PckFile assetPack in AssetPacks) {
			// assetPack.Test();
			assetPack.Install(installPath);
		}
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

		Stop();
		Unload();
	}

	private static (Assembly, AssemblyLoadContext)? LoadAssembly(string assemblyPath) {
		string assemblyDirectory = Path.GetDirectoryName(assemblyPath.AsSpan()).ToString();
		if (assemblyDirectory.Length == 0) return null;

		byte[] file = Godot.FileAccess.GetFileAsBytes(assemblyPath);
		if (file.Length == 0) {
			GD.PrintErr(Godot.FileAccess.GetOpenError());
			return null;
		}
		MemoryStream stream = new(file);

		AssemblyLoadContext assemblyLoadContext = new GodotResAssemblyLoadContext(assemblyDirectory);
		return (assemblyLoadContext.LoadFromStream(stream), assemblyLoadContext);
	}

	public void Unload() {
		foreach (PckFile asset in AssetPacks) {
			asset.Uninstall();
		}
		foreach ((_, AssemblyLoadContext context) in Assemblies) {
			context?.Unload();
		}
	}


	public void Start() {
		foreach ((Assembly assembly, _) in Assemblies) {
			assembly.GetTypes().SelectMany(type => type.GetMethods())
				.Where(method => method.Name == "Main" && method.GetParameters().Length == 0)
				.ToList().ForEach(method => method.Invoke(null, null));
		}
	}
	public void Stop() {
		// foreach (Assembly assembly in Assemblies) {
		// 	assembly.GetTypes().SelectMany(type => type.GetMethods())
		// 		.Where(method => method.Name == "Stop" && method.GetParameters().Length == 0)
		// 		.ToList().ForEach(method => method.Invoke(null, null));
		// }
	}
}