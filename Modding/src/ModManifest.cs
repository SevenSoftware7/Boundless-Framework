namespace Seven.Boundless.Modding;

using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;
using Seven.Boundless.Utility;


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

	public IEnumerable<FilePath> AssetPaths { get; set; } = [];
	public IEnumerable<FilePath> AssemblyPaths { get; set; } = [];
	public IEnumerable<string> Dependencies { get; set; } = [];


	public static ModManifest? FromYaml(in string yaml) {
		try {
			return Deserializer.Deserialize<ModManifest>(yaml);
		}
		catch {
			return null;
		}
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