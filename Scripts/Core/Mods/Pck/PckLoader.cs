namespace LandlessSkies.Core;

using Godot;
using System;
using System.Collections.Generic;
using System.Text;

public static partial class PckLoader {
	public const int GDPC = 0x43504447;

	// private static void MakeTree(string path) {
	// 	string[] parts = path.Split('/');
	// 	StringBuilder currentPath = new();

	// 	DirAccess dir = DirAccess.Open("user://") ?? throw new InvalidOperationException("Failed to access DirAccess.");
	// 	// foreach (string part in parts) {
	// 	// 	currentPath.Append(part).Append('/');
	// 	// 	string dirPath = currentPath.ToString();

	// 	// 	if (!DirAccess.DirExistsAbsolute(dirPath)) {
	// 	// 		DirAccess dir = DirAccess.Open("user://") ?? throw new InvalidOperationException("Failed to access DirAccess.");
	// 	// 		dir.MakeDir(dirPath);
	// 	// 	}
	// 	// }
	// }

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

	public static PckFile Load(string path) {
		FileAccess archive = FileAccess.Open(path, FileAccess.ModeFlags.Read);

		PckFormat format = GetFormat(archive);
		List<PckFileEntry> files = [];
		try {
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
				Entries = [.. files]
			};
		}
		catch {
			archive.Dispose();
			throw;
		}
	}
}