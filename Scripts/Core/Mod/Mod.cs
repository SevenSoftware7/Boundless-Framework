namespace LandlessSkies.Core;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Godot;

public record class Mod : IDisposable {
	private static readonly DirectoryPath ModAssetsPath = new("res://ModAssets/");
	private bool _disposed = false;
	private bool _started = false;

	public ModMetaData MetaData { get; init; }

	public (Assembly, AssemblyLoadContext)[] Assemblies { get; init; }
	public PckFile[] AssetPacks { get; init; }


	public Mod(ModMetaData metaData) {
		if (Engine.IsEditorHint()) {
			throw new InvalidOperationException("Cannot load mods in the editor.");
		}

		MetaData = metaData;

		Assemblies = metaData.AssemblyPaths
			.Select(metaData.Path.Combine)
			.Select(LoadAssembly)
			.OfType<(Assembly, AssemblyLoadContext)>()
			.ToArray();

		AssetPacks = metaData.AssetPaths
			.Select(metaData.Path.Combine)
			.Select(PckFile.Load)
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

		Stop();
		Unload();
	}

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

	public void Unload() {
		foreach (PckFile asset in AssetPacks) {
			asset.Uninstall();
		}
		foreach ((_, AssemblyLoadContext context) in Assemblies) {
			context.Unload();
		}
	}


	public void Start() {
		if (_disposed) return;

		if (_started) return;
		_started = true;

		DirectoryPath installPath = ModAssetsPath.CombineDirectory(MetaData.Name);
		foreach (PckFile assetPack in AssetPacks) {
			assetPack.Install(installPath);
		}

		foreach ((Assembly assembly, _) in Assemblies) {
			assembly.GetTypes().SelectMany(type => type.GetMethods())
				.Where(method => method.Name == "Main" && method.GetParameters().Length == 0)
				.ToList().ForEach(method => method.Invoke(null, null));
		}
	}
	public void Stop() {
		if (!_started) return;
		_started = false;

		foreach ((Assembly assembly, _) in Assemblies) {
			assembly.GetTypes().SelectMany(type => type.GetMethods())
				.Where(method => method.Name == "Stop" && method.GetParameters().Length == 0)
				.ToList().ForEach(method => method.Invoke(null, null));
		}
		foreach (PckFile assetPack in AssetPacks) {
			assetPack.Uninstall();
		}
	}
}