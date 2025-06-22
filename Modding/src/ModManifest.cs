namespace SevenDev.Boundless.Modding;

using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;
using SevenDev.Boundless.Utility;


public class ModManifest : IEquatable<ModManifest> {
	public static readonly IDeserializer Deserializer = new DeserializerBuilder()
		.WithNamingConvention(YamlDotNet.Serialization.NamingConventions.HyphenatedNamingConvention.Instance)
		.WithTypeConverter(new FilePathConverter())
		.WithEnforceRequiredMembers()
		.IgnoreUnmatchedProperties()
		.IgnoreFields()
		.Build();
	public static readonly ISerializer Serializer = new SerializerBuilder()
		.WithNamingConvention(YamlDotNet.Serialization.NamingConventions.HyphenatedNamingConvention.Instance)
		.WithTypeConverter(new FilePathConverter())
		.IgnoreFields()
		.Build();


	public required string Name { get; set; }
	public required string Version { get; set; }
	public required string Author { get; set; }
	public string Description { get; set; } = "";

	private FilePath _path;
	[YamlIgnore]
	public FilePath Path {
		get => _path;
		set {
			if (value.Directory.Protocol is not "res" or "user") {
				throw new ArgumentException("Directory must be a valid Godot path.");
			}
			_path = value;
		}
	}

	public IEnumerable<FilePath> AssetPaths { get; set; } = [];
	public IEnumerable<FilePath> AssemblyPaths { get; set; } = [];
	public IEnumerable<string> Dependencies { get; set; } = [];


	public static ModManifest FromYaml(in string yaml) {
		return Deserializer.Deserialize<ModManifest>(yaml);
	}
	public static string ToYaml(in ModManifest metaData) {
		return Serializer.Serialize(metaData);
	}


	public bool Equals(ModManifest? other) {
		if (other is null) return false;
		if (ReferenceEquals(this, other)) return true;

		return Name == other.Name
			&& Version == other.Version
			&& Author == other.Author;
	}

	public override bool Equals(object? obj) {
		if (obj is ModManifest other) {
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode() => HashCode.Combine(Name, Version, Author);
}