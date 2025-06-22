namespace SevenDev.Boundless;

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SevenDev.Boundless.Utility;


public record class PckFile : IDisposable {
	private bool _disposed = false;
	private InstalledPckFile? _installed;


	public readonly UidCache? UidCache;
	public readonly PckFormat Format;
	public readonly PckFileEntry[] Entries;


	~PckFile() {
		Dispose(false);
	}
	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}
	public void Dispose(bool disposing) {
		if (_disposed) return;

		_disposed = true;
	}


	public PckFile(FileAccess archive) {
		PckFormat format = PckFormat.Parse(archive);
		List<PckFileEntry> files = [];

		for (uint i = 0; i < format.FileCount; i++) {
			uint pathSize = archive.Get32();
			string path = Encoding.UTF8.GetString(archive.GetBuffer(pathSize));
			path = path.Replace("\0", string.Empty);
			FilePath entryPath = new(path);

			ulong offset = archive.Get64() + format.FilesBaseOffset;

			ulong size = archive.Get64();

			byte[] md5 = archive.GetBuffer(16);

			uint fileFlags = format.Version switch {
				2 => archive.Get32(),
				_ => 0
			};

			ulong currentOffset = archive.GetPosition();
			archive.Seek(offset);
			byte[] data = archive.GetBuffer((long)size);
			archive.Seek(currentOffset);

			switch (entryPath.Path) {
				case ".godot/uid_cache.bin": {
						using System.IO.MemoryStream stream = new(data);
						UidCache = new(stream);
					}
					continue;
				case ".godot/global_script_class_cache.cfg":
				case "project.binary":
					continue;
			}

			files.Add(new PckFileEntry {
				Path = entryPath,
				Data = data,
				Offset = offset,
				Size = size,
				Md5 = md5,
				Flags = fileFlags
			});
		}

		Format = format;
		Entries = [.. files];
	}

	public static PckFile Load(FilePath path) {
		using FileAccess archive = FileAccess.Open(path, FileAccess.ModeFlags.Read);

		return new(archive);
	}

	public bool Install(DirectoryPath path) {
		if (_installed is not null) return false;
		if (Entries.Any(entry => !entry.Test())) return false;

		_installed = new InstalledPckFile(this, path);

		UidCache.UpdateGlobalCache();
		return true;
	}


	public bool Uninstall() {
		if (_installed is null) return false;

		_installed.Dispose();
		_installed = null;

		UidCache.UpdateGlobalCache();
		return true;
	}

	public bool Test() {
		bool success = true;
		foreach (PckFileEntry entry in Entries) {
			success &= entry.Test();
			GD.Print($"{entry.Path}: {success}");
		}
		return success;
	}
}