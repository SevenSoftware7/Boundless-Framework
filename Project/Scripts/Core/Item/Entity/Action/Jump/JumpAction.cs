namespace LandlessSkies.Core;

using Godot;

public abstract partial class JumpAction(Entity entity) : Action(entity) {
	public Vector3 Direction { get; protected set; }
}