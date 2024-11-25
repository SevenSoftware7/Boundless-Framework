namespace LandlessSkies.Core;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Godot;

public record class Mod : IDisposable {
	private bool _disposed = false;

	public required ModMetaData MetaData { get; init; }
	public required Assembly[] Assemblies { get; init; }



	public static Mod Load(ModMetaData metaData) {
		Assembly[] assemblies = metaData.AssemblyPaths
			.Select(path => Path.Combine(metaData.Directory, path))
			.Select(LoadAssembly)
			.OfType<Assembly>()
			.ToArray();

		return new Mod {
			MetaData = metaData,
			Assemblies = assemblies
		};
	}

	public void Unload() {
		foreach (Assembly assembly in Assemblies) {
			AssemblyLoadContext.GetLoadContext(assembly)?.Unload();
		}

	}


	public void Start() {
		foreach (Assembly assembly in Assemblies) {
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

	private static Assembly? LoadAssembly(string assemblyPath) {
		string assemblyDirectory = Path.GetDirectoryName(assemblyPath.AsSpan()).ToString();
		if (assemblyDirectory.Length == 0) return null;

		byte[] file = Godot.FileAccess.GetFileAsBytes(assemblyPath);
		if (file.Length == 0) {
			GD.PrintErr(Godot.FileAccess.GetOpenError());
			return null;
		}
		MemoryStream stream = new(file);

		AssemblyLoadContext assemblyLoadContext = new GodotResAssemblyLoadContext(assemblyDirectory);
		return assemblyLoadContext.LoadFromStream(stream);
	}


	public void Dispose() {
		if (_disposed) return;
		_disposed = true;

		Stop();
		Unload();

		GC.SuppressFinalize(this);
	}
}