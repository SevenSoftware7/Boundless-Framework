namespace LandlessSkies.Core;

using Godot;
using System;

public record class PckFile : IDisposable {
	private bool _disposed = false;

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
		File.Dispose();
	}

	public required FileAccess File { get; init; }
	public required PckFormat Format { get; init; }
	public required PckFileEntry[] Entries { get; init; }

	private InstalledPckFile? _installed;


	public bool Install(string path) {
		if (_installed is not null) {
			return false;
		}

		_installed = InstalledPckFile.Install(this, path);
		// foreach (string entry in _installed?.Entries ?? []) {
		// 	GD.Print($"Installed: {entry}");
		// }
		return _installed is not null;
	}
	public void Uninstall() {
		_installed?.Dispose();
		_installed = null;
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