using System;

namespace LandlessSkies.Core;

public partial struct FilePath {
	public DirectoryPath Directory { get; init; }

	private string _fileName = string.Empty;
	public string FileName {
		readonly get => _fileName;
		init {
			_fileName = value;
			_fullFileName = null;
		}
	}

	private string _extension = string.Empty;
	public string Extension {
		readonly get => _extension;
		init {
			_extension = value;
			_fullFileName = null;
		}
	}

	private string? _fullFileName;
	public string FullFileName => _fullFileName ??= string.IsNullOrEmpty(Extension) ? FileName : $"{FileName}.{Extension}";

	private string? _path;
	public string Path => _path ??= string.IsNullOrEmpty(Directory.Path) ? FullFileName : $"{Directory.Path}/{FullFileName}";

	private string? _url;
	public string Url => _url ??= string.IsNullOrEmpty(Directory.Url) ? FullFileName : $"{Directory.Url}/{FullFileName}";

	public FilePath(DirectoryPath directory, string fileName) {
		Directory = directory;
		(FileName, Extension) = SplitFileName(fileName);
	}
	public FilePath(DirectoryPath directory, string fileName, string extension) {
		Directory = directory;
		FileName = fileName;
		Extension = extension;
	}
	public FilePath(string path) {
		int lastSeparator = path.LastIndexOf('/');
		if (lastSeparator == -1) {
			FileName = path;
			Directory = new DirectoryPath(string.Empty);
		} else {
			FileName = path[(lastSeparator + 1)..];
			Directory = new DirectoryPath(path[..lastSeparator]);
		}

		(FileName, Extension) = SplitFileName(FileName);
	}
	public FilePath(ReadOnlySpan<char> path) : this(path.ToString()) { }

	private static (string, string) SplitFileName(string fileName) {
		int lastDot = fileName.LastIndexOf('.');
		if (lastDot == -1) {
			return (fileName, string.Empty);
		}
		return (fileName[..lastDot], fileName[(lastDot + 1)..]);
	}

	public FilePath Combine(DirectoryPath other) {
		return this with {
			Directory = Directory.Combine(other)
		};
	}
	public FilePath Combine(ReadOnlySpan<char> path) {
		return Combine(new DirectoryPath(path));
	}


	public override string ToString() => Url;

	public static implicit operator string(FilePath path) => path.ToString();
}