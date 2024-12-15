using System;
using System.Text.RegularExpressions;

namespace LandlessSkies.Core;

public partial struct GodotPath {
	[GeneratedRegex(@"^/*(.*)$")]
	private static partial Regex CleanLeadingSlashes();

	[GeneratedRegex(@"^(.*)/*$")]
	private static partial Regex CleanTrailingSlashes();

	private string _protocol = string.Empty;
	public string Protocol {
		readonly get => _protocol;
		init {
			_protocol = value;
			_directoryPathWithProtocol = null;
			_fullPath = null;
		}
	}

	private string _directoryPath = string.Empty;
	public string DirectoryPath {
		readonly get => _directoryPath;
		init {
			_directoryPath = value;
			_directoryPathWithProtocol = null;
			_path = null;
		}
	}

	private string _fileName = string.Empty;
	public string FileName {
		readonly get => _fileName;
		init {
			_fileName = value;
			_fullFileName = null;
			_path = null;
		}
	}

	private string _extension = string.Empty;
	public string Extension {
		readonly get => _extension;
		init {
			_extension = value;
			_fullFileName = null;
			_path = null;
		}
	}

	private string? _fullFileName;
	public string FullFileName => _fullFileName ??= string.IsNullOrEmpty(Extension) ? FileName : $"{FileName}.{Extension}";

	private string? _directoryPathWithProtocol;
	public string DirectoryPathWithProtocol => _directoryPathWithProtocol ??= string.IsNullOrEmpty(Protocol) ? DirectoryPath : $"{Protocol}://{DirectoryPath}";

	private string? _path;
	public string Path => _path ??= string.IsNullOrEmpty(DirectoryPath) ? FullFileName : $"{DirectoryPath}/{FullFileName}";

	private string? _fullPath;
	public string FullPath => _fullPath ??= string.IsNullOrEmpty(Protocol) ? Path : $"{Protocol}://{Path}";

	public GodotPath(string path) {
		int contextSeparator = path.IndexOf("://");
		if (contextSeparator != -1) {
			Protocol = path[..contextSeparator];
			DirectoryPath = path[(contextSeparator + 3)..];
		}
		else {
			DirectoryPath = CleanLeadingSlashes().Match(path).Groups[1].Value;
		}

		int lastSeparator = DirectoryPath.LastIndexOf('/');
		if (lastSeparator == -1) {
			FileName = DirectoryPath;
			DirectoryPath = string.Empty;
		} else {
			FileName = DirectoryPath[(lastSeparator + 1)..];
			DirectoryPath = CleanTrailingSlashes().Match(DirectoryPath[..lastSeparator]).Groups[1].Value;
		}

		int lastDot = FileName.LastIndexOf('.');
		if (lastDot != -1) {
			Extension = FileName[(lastDot + 1)..];
			FileName = FileName[..lastDot];
		}
	}

	public GodotPath(ReadOnlySpan<char> path) : this(path.ToString()) { }


	public GodotPath Combine(GodotPath path) {
		return new GodotPath(string.IsNullOrEmpty(FullFileName) ? $"{FullPath}{path.Path}" : $"{DirectoryPathWithProtocol}/{path.Path}");
	}
	public GodotPath Combine(ReadOnlySpan<char> path) {
		return Combine(new GodotPath(path));
	}

	public override string ToString() => FullPath;

	public static implicit operator string(GodotPath path) => path.ToString();
}