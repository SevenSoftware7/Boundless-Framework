namespace SevenDev.Boundless;

using Godot;
using System.Collections;
using System.Security.Cryptography;
using SevenDev.Boundless.Utility;


public record class PckFileEntry {
	public required FilePath Path { get; init; }
	public required byte[] Data { get; init; }
	public required ulong Offset { get; init; }
	public required ulong Size { get; init; }
	public required byte[] Md5 { get; init; }

	public required uint Flags { get; init; }

	private byte[]? _digest;
	public byte[] Digest => _digest ??= MD5.HashData(Data);


	public bool Test() => StructuralComparisons.StructuralEqualityComparer.Equals(Digest, Md5);

	public static FilePath GetExtractPath(DirectoryPath extractPath, PckFileEntry fileEntry) {
		if (fileEntry.Path.Directory.Path.StartsWith(".godot")) {
			return fileEntry.Path with {
				Directory = fileEntry.Path.Directory with {
					Protocol = "res"
				}
			};
		}
		return extractPath.Combine(fileEntry.Path);
	}

	public bool Extract(FilePath path) {
		if (!Test()) return false;

		DirAccess.MakeDirRecursiveAbsolute(path.Directory);
		using FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Write);

		file?.StoreBuffer(Data);
		return file is not null;
	}
}