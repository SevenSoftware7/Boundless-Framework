namespace LandlessSkies.Core;

using Godot;
using SevenDev.Boundless.Persistence;

[Tool]
public partial class GenericResourceItemKey<[MustBeVariant] T> : ResourceItemKey, IItemKeyProvider where T : IItem {
	public GenericResourceItemKey() : this(null) { }
	public GenericResourceItemKey(string? key = null) : base(key) { }
}