namespace SevenDev.Boundless;

using System.Runtime.Loader;
using System.Reflection;
using System.IO;
using Godot;
using SevenDev.Boundless.Utility;


public sealed class GodotResAssemblyLoadContext : AssemblyLoadContext {
	private readonly DirectoryPath _assemblyPath;

	public GodotResAssemblyLoadContext(DirectoryPath assemblyPath) : base(isCollectible: true) {
		_assemblyPath = assemblyPath;
	}

	protected override Assembly? Load(AssemblyName assemblyName) {
		try {
			Assembly? defaultAssembly = Assembly.Load(assemblyName);
			return defaultAssembly;
		}
		catch {
			string? assemblyPath = _assemblyPath.CombineDirectory($"{assemblyName.Name}.dll");
			byte[]? file = Godot.FileAccess.GetFileAsBytes(assemblyPath);
			if (file.Length == 0) {
				GD.PrintErr(Godot.FileAccess.GetOpenError());
				return null;
			}

			return LoadFromStream(new MemoryStream(file));
		}
	}

	protected override nint LoadUnmanagedDll(string unmanagedDllName) {
		string? libraryPath = _assemblyPath.CombineFile($"{unmanagedDllName}.dll");
		byte[]? file = Godot.FileAccess.GetFileAsBytes(libraryPath);
		if (file.Length == 0) {
			GD.PrintErr(Godot.FileAccess.GetOpenError());
			return 0;
		}

		string tempPath = Path.GetTempFileName();
		File.WriteAllBytes(tempPath, file);

		nint loaded = LoadUnmanagedDllFromPath(tempPath);
		File.Delete(tempPath);

		return loaded;
	}
}