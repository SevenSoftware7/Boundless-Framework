namespace LandlessSkies.Core;

using System;
using Godot;
using SevenDev.Boundless.Persistence;

[Tool]
[GlobalClass]
public partial class ResourceItemKey : Resource, IItemKeyProvider {
	[Export] public string String {
		get => ItemKey?.String ?? string.Empty;
		private set {
			ItemKey? newKey = null;
			try {
				newKey = new(value);
			}
			catch (Exception e) {
				if (ItemKey is not null) GD.PrintErr($"Invalid Item Key: {value} - {e}");
			}
			finally {
				ItemKey?.Dispose();
				ItemKey = newKey;
			}
		}
	}
	public ItemKey? ItemKey { get; private set; }


	public ResourceItemKey() : this(null) { }
	public ResourceItemKey(string? @string = null) : base() {
		ItemKey = @string is null ? null : new(@string);
	}
}