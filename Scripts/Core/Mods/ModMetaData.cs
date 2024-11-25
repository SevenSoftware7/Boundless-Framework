namespace LandlessSkies.Core;

using System.Collections.Generic;
using YamlDotNet.Serialization;

public record class ModMetaData {
	public required string Name { get; set; }
	public required string Version { get; set; }
	public required string Author { get; set; }
	public string Description { get; set; } = "";

	public string Directory = "";

	public IEnumerable<string> AssemblyPaths { get; set; } = [];
	public IEnumerable<string> AssetDirectories { get; set; } = [];


	public static ModMetaData FromYaml(string yaml) {
		IDeserializer deserializer = new DeserializerBuilder()
			.WithNamingConvention(YamlDotNet.Serialization.NamingConventions.HyphenatedNamingConvention.Instance)
			.WithEnforceRequiredMembers()
			.IgnoreUnmatchedProperties()
			.IgnoreFields()
			.Build();

		return deserializer.Deserialize<ModMetaData>(yaml);
	}
}