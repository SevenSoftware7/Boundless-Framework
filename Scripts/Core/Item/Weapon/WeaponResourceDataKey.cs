namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public sealed partial class WeaponResourceDataKey : Resource, IDataKeyProvider<Weapon> {
	[Export] public string Key {
		get => key;
		private set => key = value.ToSnakeCase();
	}
	private string key = string.Empty;
}