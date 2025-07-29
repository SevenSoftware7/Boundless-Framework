namespace Seven.Boundless.Modding;

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Seven.Boundless.Utility;
using System.IO;

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


	public PckFile(BinaryReader reader) {
		PckFormat format = PckFormat.Parse(reader);
		List<PckFileEntry> files = [];

		for (uint i = 0; i < format.FileCount; i++) {
			uint pathSize = reader.ReadUInt32();
			byte[] pathBytes = reader.ReadBytes((int)pathSize);
			FilePath path = new(Encoding.UTF8.GetString(pathBytes).Replace("\0", string.Empty));

			ulong offset = reader.ReadUInt64() + format.FilesBaseOffset;

			ulong size = reader.ReadUInt64();

			byte[] md5 = reader.ReadBytes(16);

			uint fileFlags = format.Version switch {
				2 => reader.ReadUInt32(),
				_ => 0
			};

			long currentOffset = reader.BaseStream.Position;
			reader.BaseStream.Position = (long)offset;
			byte[] data = reader.ReadBytes((int)size);
			reader.BaseStream.Position = currentOffset;

			switch (path.Path) {
				case ".godot/uid_cache.bin": {
						using MemoryStream stream = new(data);
						UidCache = new(stream);
					}
					continue;
				case ".godot/global_script_class_cache.cfg":
				case "project.binary":
					continue;
			}

			files.Add(new PckFileEntry {
				Path = path,
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

	public static PckFile Load(Stream contents) {
		using BinaryReader reader = new(contents);

		return new(reader);
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