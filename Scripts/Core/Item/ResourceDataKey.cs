namespace LandlessSkies.Core;

using Godot;
using SevenDev.Boundless.Persistence;

[Tool]
[GlobalClass]
public partial class ResourceDataKey : Resource, IDataKeyProvider {
	[Export] public string Key {
		get => _key ?? ResourcePath;
		private set {
			if (string.IsNullOrEmpty(value)) {
				if (_key is not null) GD.PrintErr($"Key cannot be null or empty: {ResourcePath}");
				return;
			}
			_key = value.ToSnakeCase();
		}
	}
	private string? _key;


	public ResourceDataKey() : this(null) { }
	public ResourceDataKey(string? key = null) : base() {
		_key = key?.ToSnakeCase();
	}
}