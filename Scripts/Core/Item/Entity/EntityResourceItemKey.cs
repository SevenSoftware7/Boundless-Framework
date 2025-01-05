namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public sealed partial class EntityResourceItemKey : GenericResourceItemKey<Entity> {
	public EntityResourceItemKey() : base() {}
	public EntityResourceItemKey(string? key) : base(key) {}
}