namespace LandlessSkies.Core;

using Godot;
using SevenDev.Boundless.Persistence;

[Tool]
[GlobalClass]
public sealed partial class EntityResourceDataKey : Resource, IDataKeyProvider<Entity> {
	[Export] public string Key {
		get => key;
		private set => key = value.ToSnakeCase();
	}
	private string key = string.Empty;
}