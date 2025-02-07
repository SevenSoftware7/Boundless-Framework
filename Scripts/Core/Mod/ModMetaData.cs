namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using Godot;
using YamlDotNet.Serialization;
using SevenDev.Boundless.Utility;

public record class ModMetaData {
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
			return new Mod(this);
		}
		catch (Exception e) {
			GD.PrintErr(e.Message);
			return null;
		}
	}
}