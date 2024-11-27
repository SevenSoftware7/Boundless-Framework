using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

public static class PckExtractor {
	public const int GDPC = 0x43504447;

	private static void MakeTree(string path) {
		string[] parts = path.Split('/');
		StringBuilder currentPath = new();

		foreach (string part in parts) {
			currentPath.Append(part).Append('/');
			string dirPath = currentPath.ToString();

			if (!DirAccess.DirExistsAbsolute(dirPath)) {
				DirAccess dir = DirAccess.Open("user://") ?? throw new InvalidOperationException("Failed to access DirAccess.");
				dir.MakeDir(dirPath);
			}
		}
	}

	private static PckFormat GetFormat(FileAccess archive) {
		uint magic = archive.Get32();
		if (magic != GDPC) {
			archive.Seek(archive.GetLength() - 4);
			magic = archive.Get32();

			if (magic != GDPC) {
				throw new InvalidOperationException("Invalid file format.");
			}

			archive.Seek(archive.GetPosition() - 12);
			ulong ds = archive.Get64();

			archive.Seek(archive.GetPosition() - ds - 8);
			magic = archive.Get32();

			if (magic != GDPC) {
				throw new InvalidOperationException("Invalid file format.");
			}

			// isEmbedded = true;
		}

		uint formatVersion = archive.Get32();
		uint verMajor = archive.Get32();
		uint verMinor = archive.Get32();
		uint verRev = archive.Get32();

		// Determine files base offset
		(uint fileFlags, ulong filesBaseOffset) = formatVersion switch {
			0 or 1 => default,
			2 => (archive.Get32(), archive.Get64()),
			_ => throw new InvalidOperationException("Unsupported format version.")
		};

		// Skip reserved bytes
		archive.Seek(archive.GetPosition() + 16 * sizeof(uint));

		// Read file count
		uint fileCount = archive.Get32();
		GD.Print($"PCK format version {formatVersion}, {fileCount} files");


		return new PckFormat {
			Version = formatVersion,
			Major = verMajor,
			Minor = verMinor,
			Revision = verRev,
			Flags = fileFlags,
			FilesBaseOffset = filesBaseOffset,
			FileCount = fileCount,
			IsEmbedded = formatVersion == 0 || formatVersion == 1
		};
	}

	public static PckFile Load(FileAccess archive) {
		PckFormat format = GetFormat(archive);

		List<PckFileEntry> files = [];

		for (uint i = 0; i < format.FileCount; i++) {
			uint nameSize = archive.Get32();
			string name = Encoding.UTF8.GetString(archive.GetBuffer(nameSize));

			ulong offset = archive.Get64() + format.FilesBaseOffset;

			ulong size = archive.Get64();

			byte[] md5 = archive.GetBuffer(16);

			uint fileFlags = format.Version switch {
				2 => archive.Get32(),
				_ => 0
			};

			files.Add(new PckFileEntry {
				File = archive,
				Path = name,
				Offset = offset,
				Size = size,
				Md5 = md5,
				Flags = fileFlags
			});
		}

		return new PckFile {
			File = archive,
			Format = format,
			Files = files
		};
	}
	public static PckFile Load(string path) {
		FileAccess archive = FileAccess.Open(path, FileAccess.ModeFlags.Read);
		return Load(archive);
	}


	public struct PckFileEntry {
		public readonly required FileAccess File { get; init; }
		public readonly required string Path { get; init; }
		public readonly required ulong Offset { get; init; }
		public readonly required ulong Size { get; init; }
		public readonly required byte[] Md5 { get; init; }

		public readonly required uint Flags { get; init; }

		private byte[]? _data;
		public byte[] Data {
			get {
				if (_data is not null) {
					return _data;
				}
				File.Seek(Offset);
				return _data = File.GetBuffer((long)Size);
			}
		}

		private byte[]? _digest;
		public byte[] Digest {
			get {
				if (_digest is not null) {
					return _digest;
				}

				byte[] data = Data;
				return _digest = MD5.HashData(data);
			}
		}


		public bool Test() {
			byte[] digest = Digest;
			return StructuralComparisons.StructuralEqualityComparer.Equals(digest, Md5);
		}

		public bool Extract(string path) {
			string directory = path.GetBaseDir();
			if (!string.IsNullOrEmpty(directory)) {
				MakeTree(directory);
			}

			if (!Test()) {
				return false;
			}
			byte[] data = Data;

			using FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
			file.StoreBuffer(data);

			return true;
		}
	}

	public readonly record struct PckFormat {
		public uint Version { get; init; }

		public uint Major { get; init; }
		public uint Minor { get; init; }
		public uint Revision { get; init; }

		public uint Flags { get; init; }
		public ulong FilesBaseOffset { get; init; }
		public uint FileCount { get; init; }

		public bool IsEmbedded { get; init; }
		public readonly bool IsEncrypted => (Flags & 0x1) != 0;
	}

	public readonly record struct PckFile {
		public required FileAccess File { get; init; }
		public required PckFormat Format { get; init; }
		public required IEnumerable<PckFileEntry> Files { get; init; }

		// public bool Extract(string path) {
		// 	bool success = true;
		// 	foreach (PckFileEntry entry in Files) {
		// 		success &= entry.Extract(path + entry.Path);
		// 	}
		// 	return success;
		// }
		public bool Test() {
			bool success = true;
			foreach (PckFileEntry entry in Files) {
				success &= entry.Test();
				GD.Print($"{entry.Path}: {success}");
			}
			return success;
		}
	}
}