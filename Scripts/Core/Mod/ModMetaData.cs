namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using Godot;
using YamlDotNet.Serialization;
using SevenDev.Boundless.Utility;

public class ModMetaData : IEquatable<ModMetaData> {
	public static readonly IDeserializer Deserializer = new DeserializerBuilder()
		.WithNamingConvention(YamlDotNet.Serialization.NamingConventions.HyphenatedNamingConvention.Instance)
		.WithTypeConverter(new FilePathConverter())
		.WithEnforceRequiredMembers()
		.IgnoreUnmatchedProperties()
		.IgnoreFields()
		.Build();


	public required string Name { get; set; }
	public required string Version { get; set; }
	public required string Author { get; set; }
	public string Description { get; set; } = "";

	private DirectoryPath _path;
	public DirectoryPath Path {
		get => _path;
		set {
			if (value.Protocol is not "res" or "user") {
				throw new ArgumentException("Directory must be a valid Godot path.");
			}
			_path = value;
		}
	}

	public IEnumerable<FilePath> AssetPaths { get; set; } = [];
	public IEnumerable<FilePath> AssemblyPaths { get; set; } = [];


	public static ModMetaData FromYaml(in string yaml) {
		return Deserializer.Deserialize<ModMetaData>(yaml);
	}


	public Mod? Load() {
		try {
			return Mod.Load(this);
		}
		catch (Exception e) {
			GD.PrintErr(e.Message);
			return null;
		}
	}

	bool IEquatable<ModMetaData>.Equals(ModMetaData? other) {
		if (other is null) return false;
		if (ReferenceEquals(this, other)) return true;

		return Name == other.Name
			&& Version == other.Version
			&& Author == other.Author;
	}

	public override bool Equals(object? obj) {
		if (obj is ModMetaData other) {
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode() => HashCode.Combine(Name, Version, Author);
}