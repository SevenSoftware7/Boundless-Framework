namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public sealed partial class EntityResourceDataKey : GenericResourceDataKey<Entity> {
	public EntityResourceDataKey() : base() {}
	public EntityResourceDataKey(string? key) : base(key) {}
}