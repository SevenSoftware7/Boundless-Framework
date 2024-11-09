namespace LandlessSkies.Core;

using Godot;
using SevenDev.Boundless.Persistence;

[Tool]
public sealed partial class ResourceDataKey<T> : Resource, IDataKeyProvider<T> where T : IItem<T> {
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