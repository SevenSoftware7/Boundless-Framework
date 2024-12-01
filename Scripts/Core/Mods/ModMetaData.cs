namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Godot;
using YamlDotNet.Serialization;

public record class ModMetaData {
	public required string Name { get; set; }
	public required string Version { get; set; }
	public required string Author { get; set; }
	public string Description { get; set; } = "";

	public string Directory {
		get => _directory;
		set {
			if (!value.StartsWith("res://") && !value.StartsWith("user://")) {
				throw new ArgumentException("Directory must be a valid Godot path.");
			}
			_directory = value;
		}
	}
	private string _directory = "";

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