namespace LandlessSkies.Core;

using Godot;

public sealed class JumpActionBuilder(JumpActionInfo Info, Vector3 direction) : EntityActionBuilder {
	protected internal override JumpAction Build(Entity entity) => Info.Create(entity, direction);
}