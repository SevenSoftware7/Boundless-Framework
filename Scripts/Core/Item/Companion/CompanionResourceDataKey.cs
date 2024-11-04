namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public sealed partial class CompanionResourceDataKey : Resource, IDataKeyProvider<Companion> {
	[Export] public string Key {
		get => key;
		private set => key = value.ToSnakeCase();
	}
	private string key = string.Empty;
}