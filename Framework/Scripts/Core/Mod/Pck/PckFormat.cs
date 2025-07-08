namespace SevenDev.Boundless.Modding;

using System;
using System.IO;
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


	public static PckFormat Parse(BinaryReader reader) {
		uint magic = reader.ReadUInt32();
		if (magic != GDPC) {
			throw new InvalidOperationException("Invalid file format.");
		}

		uint formatVersion = reader.ReadUInt32();
		uint verMajor = reader.ReadUInt32();
		uint verMinor = reader.ReadUInt32();
		uint verRev = reader.ReadUInt32();

		// Determine files base offset
		(uint fileFlags, ulong filesBaseOffset) = formatVersion switch {
			0 or 1 => default,
			2 => (reader.ReadUInt32(), reader.ReadUInt64()),
			_ => throw new InvalidOperationException("Unsupported format version.")
		};

		// Skip reserved bytes
		reader.BaseStream.Seek(16 * sizeof(uint), SeekOrigin.Current);

		// Read file count
		uint fileCount = reader.ReadUInt32();


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