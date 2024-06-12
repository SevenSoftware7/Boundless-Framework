namespace LandlessSkies.Core;

using Godot;

public abstract partial class JumpAction(Entity entity) : EntityAction(entity) {
	public Vector3 Direction { get; protected set; }
}