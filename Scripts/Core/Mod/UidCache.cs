namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Godot;

public record class UidCache {
	public static readonly FilePath GlobalCachePath = new("res://.godot/uid_cache.bin");
	public static readonly UidCache GlobalCache = ParseFile(GlobalCachePath) ?? new UidCache();


	private readonly Dictionary<long, FilePath> _uidToPath = [];
	private readonly Dictionary<FilePath, long> _pathToUid = [];


	public UidCache() { }

	public UidCache(Godot.FileAccess file) {
		uint count = file.Get32();
		for (uint i = 0; i < count; i++) {
			long uid = (long)file.Get64();
			uint pathSize = file.Get32();
			FilePath path = new(Encoding.UTF8.GetString(file.GetBuffer(pathSize)));

			Add(uid, path);
		}
	}

	public UidCache(Stream stream) {
		_uidToPath = [];

		byte[] buffer = new byte[8];
		stream.Read(buffer, 0, 4);
		uint count = BitConverter.ToUInt32(buffer, 0);

		for (uint i = 0; i < count; i++) {
			stream.Read(buffer, 0, 8);
			long uid = BitConverter.ToInt64(buffer, 0);

			stream.Read(buffer, 0, 4);
			uint pathSize = BitConverter.ToUInt32(buffer, 0);

			buffer = new byte[pathSize];
			stream.Read(buffer, 0, (int)pathSize);
			FilePath path = new(Encoding.UTF8.GetString(buffer));


			Add(uid, path);
		}
	}

	public static UidCache? ParseFile(FilePath path) {
		using Godot.FileAccess file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
		if (file is null) {
			GD.PrintErr(Godot.FileAccess.GetOpenError());
			return null;
		}

		return new UidCache(file);
	}

	public void WriteToFile(FilePath path) {
		using Godot.FileAccess file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Write);
		if (file is null) {
			GD.PrintErr(Godot.FileAccess.GetOpenError());
			return;
		}

		file.Store32((uint)_uidToPath.Count);
		foreach (KeyValuePair<long, FilePath> pair in _uidToPath) {
			file.Store64((ulong)pair.Key);

			byte[] pathBytes = Encoding.UTF8.GetBytes(pair.Value.Url);
			file.Store32((uint)pathBytes.Length);
			file.StoreBuffer(pathBytes);
		}
	}


	public bool TryGetPath(long uid, out FilePath path) {
		return _uidToPath.TryGetValue(uid, out path);
	}
	public bool TryGetUid(FilePath path, out long uid) {
		return _pathToUid.TryGetValue(path, out uid);
	}

	public void Add(long uid, FilePath path) {
		path = path with { Directory = path.Directory with { Protocol = "res"} };
		_uidToPath[uid] = path;
		_pathToUid[path] = uid;
	}

	public void Remove(FilePath path) {
		if (_pathToUid.TryGetValue(path, out long uid)) {
			_uidToPath.Remove(uid);
			_pathToUid.Remove(path);
		}
	}

	public void Remove(long uid) {
		if (_uidToPath.TryGetValue(uid, out FilePath path)) {
			_uidToPath.Remove(uid);
			_pathToUid.Remove(path);
		}
	}

	public void Clear() {
		_uidToPath.Clear();
		_pathToUid.Clear();
	}
}