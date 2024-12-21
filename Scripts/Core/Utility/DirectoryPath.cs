using System;
using System.Text.RegularExpressions;

namespace LandlessSkies.Core;

public partial struct DirectoryPath : IEquatable<DirectoryPath> {
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
			_path = CleanSlashes().Match(value).Groups[1].Value;
			_url = null;
		}
	}

	private string? _url;
	public string Url => _url ??= string.IsNullOrEmpty(Protocol) ? Path : $"{Protocol}://{Path}";

	public DirectoryPath(string path) {
		int contextSeparator = path.IndexOf("://");
		if (contextSeparator != -1) {
			Protocol = path[..contextSeparator];
			Path = path[(contextSeparator + 3)..];
		}
		else {
			Path = path;
		}
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

	public readonly bool Equals(DirectoryPath other) {
		return Path == other.Path;
	}
	public override readonly bool Equals(object? obj) {
		return obj is DirectoryPath other && Equals(other);
	}
	public override readonly int GetHashCode() {
		return Path.GetHashCode();
	}


	public static bool operator ==(DirectoryPath left, DirectoryPath right) => left.Equals(right);
	public static bool operator !=(DirectoryPath left, DirectoryPath right) => !left.Equals(right);


	public override string ToString() => Url;

	public static implicit operator string(DirectoryPath path) => path.ToString();
}