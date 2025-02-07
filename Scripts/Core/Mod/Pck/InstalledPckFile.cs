using System;
using Godot;
using SevenDev.Boundless.Utility;

namespace LandlessSkies.Core;

public record class InstalledPckFile : IDisposable {
	private bool _disposed = false;

	public PckFile File { get; init; }
	public FilePath[] Paths { get; init; }


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


	public InstalledPckFile(PckFile file, DirectoryPath path) {
		File = file;
		Paths = new FilePath[file.Entries.Length];

		for (uint i = 0; i < file.Entries.Length; i++) {
			PckFileEntry entry = file.Entries[i];

			FilePath extractPath = PckFileEntry.GetExtractPath(path, entry);
			Paths[i] = extractPath;

			entry.Extract(extractPath);

			if (
				!entry.Path.Directory.Path.StartsWith(".godot")
				&& (file.UidCache?.TryGetUid(entry.Path, out long uid) ?? false)
			) {
				UidCache.AdditionalCache.Add(uid, extractPath);
			}
		}
	}


	public void Clean() {
		foreach (FilePath entry in Paths) {
			using DirAccess? dir = DirAccess.Open(entry.Directory.Url);
			dir?.Remove(entry);

			UidCache.AdditionalCache.Remove(entry);
		}
	}
}