namespace Seven.Boundless.Modding;

using System.Runtime.Loader;
using System.Reflection;
using System.IO;
using Godot;
using Seven.Boundless.Utility;
using System.IO.Compression;

public sealed class ZipFileAssemblyLoadContext : AssemblyLoadContext {
	private readonly ZipArchive _zipArchive;
	private readonly DirectoryPath _assemblyPath;

	public ZipFileAssemblyLoadContext(ZipArchive zipArchive, DirectoryPath assemblyPath) : base(isCollectible: true) {
		_zipArchive = zipArchive;
		_assemblyPath = assemblyPath;
	}

	protected override Assembly? Load(AssemblyName assemblyName) {
		try {
			Assembly? defaultAssembly = Assembly.Load(assemblyName);
			return defaultAssembly;
		}
		catch {
			FilePath assemblyFilePath = _assemblyPath.CombineFile($"{assemblyName.Name}.dll");
			if (_zipArchive.GetEntry(assemblyFilePath) is not ZipArchiveEntry entry) {
				GD.PrintErr($"[Boundless.Modding]: Assembly {assemblyName.Name} not found in zip file {_zipArchive}");
				return null;
			}
			using Stream stream = entry.Open();

			return LoadFromStream(stream);
		}
	}

	protected override nint LoadUnmanagedDll(string unmanagedDllName) {
		FilePath assemblyFilePath = _assemblyPath.CombineFile($"{unmanagedDllName}.dll");
		if (_zipArchive.GetEntry(assemblyFilePath) is not ZipArchiveEntry entry) {
			GD.PrintErr($"[Boundless.Modding]: Unmanaged DLL {unmanagedDllName} not found in zip file {_zipArchive}");
			return 0;
		}
		using Stream stream = entry.Open();

		string tempPath = Path.GetTempFileName();
		using (FileStream fileStream = new(tempPath, FileMode.Create)) {
			stream.CopyTo(fileStream);
		}

		nint loaded = LoadUnmanagedDllFromPath(tempPath);
		File.Delete(tempPath);

		return loaded;
	}
}