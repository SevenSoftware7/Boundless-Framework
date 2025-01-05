namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public sealed partial class EntityCostumeResourceItemKey : CostumeResourceItemKey {
	public EntityCostumeResourceItemKey() : base() {}
	public EntityCostumeResourceItemKey(string? key) : base(key) {}
}