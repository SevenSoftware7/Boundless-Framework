namespace SevenDev.Boundless;

using System;
using Godot;
using SevenDev.Boundless.Persistence;

[Tool]
[GlobalClass]
public partial class ResourceItemKey : Resource {
	[Export] public string String {
		get => ItemKey?.String ?? string.Empty;
		private set {
			if (string.IsNullOrWhiteSpace(value)) {
				ItemKey = null;
				return;
			}

			try {
				ItemKey = new(value);
			}
			catch (Exception e) {
				GD.PrintErr($"Invalid Item Key ({value}) - {e}");
				ItemKey = null;
			}
		}
	}
	public ItemKey? ItemKey { get; set; }


	public ResourceItemKey() : base() { }
	public ResourceItemKey(string @string) : this() {
		String = @string ?? string.Empty;
	}
	public ResourceItemKey(ItemKey key) : this() {
		ItemKey = key;
	}
}