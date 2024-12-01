namespace LandlessSkies.Core;

using Godot;
using System;
using System.Collections;
using System.Security.Cryptography;

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
			ulong currentOffset = File.GetPosition();
			File.Seek(Offset);
			_data = File.GetBuffer((long)Size);
			File.Seek(currentOffset);
			return _data;
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


	public bool Test() => StructuralComparisons.StructuralEqualityComparer.Equals(Digest, Md5);

	public bool Extract(string path) {
		if (!Test()) {
			return false;
		}
		byte[] data = Data;

		DirAccess.MakeDirRecursiveAbsolute(System.IO.Path.GetDirectoryName(path.AsSpan()).ToString());
		using FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
		file.StoreBuffer(data);

		return true;
	}
}