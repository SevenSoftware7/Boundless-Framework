namespace LandlessSkies.Core;

using Godot;

public sealed class JumpActionBuilder(JumpActionInfo Info, Vector3 direction) : ActionBuilder {
	protected internal override JumpAction Create(Entity entity) => Info.Create(entity, direction);
}