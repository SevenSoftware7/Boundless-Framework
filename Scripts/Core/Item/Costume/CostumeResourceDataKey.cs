namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public partial class CostumeResourceDataKey : GenericResourceDataKey<Costume> {
	public CostumeResourceDataKey() : base() {}
	public CostumeResourceDataKey(string? key) : base(key) {}
}