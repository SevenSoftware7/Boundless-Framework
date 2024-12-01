namespace LandlessSkies.Core;

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