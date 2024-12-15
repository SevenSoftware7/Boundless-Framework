namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using Godot;
using YamlDotNet.Serialization;

public record class ModMetaData {
	public required string Name { get; set; }
	public required string Version { get; set; }
	public required string Author { get; set; }
	public string Description { get; set; } = "";

	private GodotPath _path;
	public GodotPath Path {
		get => _path;
		set {
			if (value.Protocol is not "res" or "user") {
				throw new ArgumentException("Directory must be a valid Godot path.");
			}
			_path = value;
		}
	}

	public IEnumerable<string> AssetPaths { get; set; } = [];
	public IEnumerable<string> AssemblyPaths { get; set; } = [];


	public static ModMetaData FromYaml(string yaml) {
		IDeserializer deserializer = new DeserializerBuilder()
			.WithNamingConvention(YamlDotNet.Serialization.NamingConventions.HyphenatedNamingConvention.Instance)
			.WithEnforceRequiredMembers()
			.IgnoreUnmatchedProperties()
			.IgnoreFields()
			.Build();

		return deserializer.Deserialize<ModMetaData>(yaml);
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