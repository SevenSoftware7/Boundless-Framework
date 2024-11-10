namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public sealed partial class EntityCostumeResourceDataKey : CostumeResourceDataKey {
	public EntityCostumeResourceDataKey() : base() {}
	public EntityCostumeResourceDataKey(string? key) : base(key) {}
}