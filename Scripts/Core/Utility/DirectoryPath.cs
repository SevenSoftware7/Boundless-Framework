using System;
using System.Text.RegularExpressions;

namespace LandlessSkies.Core;

public partial struct DirectoryPath {
	[GeneratedRegex(@"^/*(.*?)/*$")]
	private static partial Regex CleanSlashes();

	private string _protocol = string.Empty;
	public string Protocol {
		readonly get => _protocol;
		init {
			_protocol = value;
			_url = null;
		}
	}

	private string _path = string.Empty;
	public string Path {
		readonly get => _path;
		init {
			_path = value;
			_url = null;
		}
	}

	private string? _url;
	public string Url => _url ??= string.IsNullOrEmpty(Protocol) ? Path : $"{Protocol}://{Path}";

	public DirectoryPath(string path) {
		Path = path;

		int contextSeparator = Path.IndexOf("://");
		if (contextSeparator != -1) {
			Protocol = Path[..contextSeparator];
			Path = Path[(contextSeparator + 3)..];
		}
		Path = CleanSlashes().Match(Path).Groups[1].Value;
	}

	public DirectoryPath(ReadOnlySpan<char> path) : this(path.ToString()) { }


	public DirectoryPath Combine(DirectoryPath other) {
		return this with {
			Path = string.IsNullOrEmpty(Path) ? other.Path :
				   string.IsNullOrEmpty(other.Path) ? Path :
				   $"{Path}/{other.Path}"
		};
	}
	public DirectoryPath CombineDirectory(ReadOnlySpan<char> path) {
		return Combine(new DirectoryPath(path));
	}

	public FilePath Combine(FilePath file) {
		return file with {
			Directory = Combine(file.Directory)
		};
	}
	public FilePath CombineFile(ReadOnlySpan<char> path) {
		return Combine(new FilePath(path));
	}

	public override string ToString() => Url;

	public static implicit operator string(DirectoryPath path) => path.ToString();
}