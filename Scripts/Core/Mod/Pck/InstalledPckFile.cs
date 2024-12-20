using System;
using Godot;

namespace LandlessSkies.Core;

public record class InstalledPckFile : IDisposable {
	private bool _disposed = false;

	public required string Path { get; init; }
	public required PckFile File { get; init; }
	public required FilePath[] Entries { get; init; }


	~InstalledPckFile() {
		Dispose(false);
	}
	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing) {
		if (_disposed) return;
		_disposed = true;

		Clean();
	}


	private static void RemoveFile(FilePath path) {
		DirAccess? dir = DirAccess.Open(path.Url);
		if (dir is null) {
			return;
		}
		dir.Remove(path);
	}

	public void Clean() {
		foreach (FilePath entry in Entries) {
			RemoveFile(entry);
			GD.Print($"Uninstalled: {entry}");
		}
	}
}