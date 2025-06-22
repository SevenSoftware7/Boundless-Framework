namespace SevenDev.Boundless.Modding;

using System;
using Godot;


public readonly record struct PckFormat {
	public const int GDPC = 0x43504447;

	public uint Version { get; init; }

	public uint Major { get; init; }
	public uint Minor { get; init; }
	public uint Revision { get; init; }

	public uint Flags { get; init; }
	public ulong FilesBaseOffset { get; init; }
	public uint FileCount { get; init; }

	public bool IsEmbedded { get; init; }
	public readonly bool IsEncrypted => (Flags & 0x1) != 0;


	public static PckFormat Parse(FileAccess archive) {
		archive.Seek(0);

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
}