namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public sealed partial class CostumeResourceDataKey : Resource, IDataKeyProvider<Costume> {
	[Export] public string Key {
		get => key;
		private set => key = value.ToSnakeCase();
	}
	private string key = string.Empty;
}