using System;
using Godot;

namespace LandlessSkies.Core;

public record class InstalledPckFile : IDisposable {
	private bool _disposed = false;

	public required string Path { get; init; }
	public required PckFile File { get; init; }
	public required string[] Entries { get; init; }


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


	private static string GetExtractPath(string path, PckFileEntry fileEntry) {
		if (fileEntry.Path.StartsWith(".godot/")) {
			return fileEntry.Path;
		}
		return System.IO.Path.Combine(path, fileEntry.Path);
	}
	private static void RemoveFile(string path) {
		string directory = System.IO.Path.GetDirectoryName(path.AsSpan()).ToString();
		DirAccess? dir = DirAccess.Open(directory);
		if (dir is null) {
			return;
		}
		dir.Remove(path);
	}

	public static InstalledPckFile? Install(PckFile file, string path) {
		bool success = true;

		string[] extractedEntries = new string[file.Entries.Length];
		uint extractedCount = 0;
		for (; extractedCount < file.Entries.Length; extractedCount++) {
			PckFileEntry entry = file.Entries[extractedCount];
			string extractPath = GetExtractPath(path, entry);
			extractedEntries[extractedCount] = extractPath;

			success = entry.Extract(extractPath);
			if (!success) {
				break;
			}
		}

		if (!success) {
			foreach (string entry in extractedEntries) {
				RemoveFile(entry);
			}
			return null;
		}

		return new InstalledPckFile {
			Path = path,
			File = file,
			Entries = extractedEntries
		};
	}

	public void Clean() {
		foreach (string entry in Entries) {
			RemoveFile(entry);
			GD.Print($"Uninstalled: {entry}");
		}
	}
}