namespace LandlessSkies.Core;

using Godot;
using SevenDev.Boundless.Persistence;

[Tool]
public partial class GenericResourceDataKey<[MustBeVariant] T> : ResourceDataKey, IDataKeyProvider<T> where T : IItem<T> {
	public GenericResourceDataKey() : this(null) { }
	public GenericResourceDataKey(string? key = null) : base(key) { }
}