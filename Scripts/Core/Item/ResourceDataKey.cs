namespace LandlessSkies.Core;

using Godot;
using SevenDev.Boundless.Persistence;

[Tool]
[GlobalClass]
public partial class ResourceDataKey : Resource, IDataKeyProvider {
	[Export] public string Key {
		get => key;
		private set => key = value.ToSnakeCase();
	}
	private string key = string.Empty;


	public ResourceDataKey() : this(null) { }
	public ResourceDataKey(string? key = null) : base() {
		Key = key ?? string.Empty;
	}
}