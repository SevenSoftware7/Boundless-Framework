namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public partial class CostumeResourceItemKey : GenericResourceItemKey<Costume> {
	public CostumeResourceItemKey() : base() {}
	public CostumeResourceItemKey(string? key) : base(key) {}
}