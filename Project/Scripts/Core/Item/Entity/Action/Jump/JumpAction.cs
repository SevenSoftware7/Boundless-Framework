namespace LandlessSkies.Core;

using Godot;

public abstract partial class JumpAction(Entity entity) : Action(entity) {
	public Vector3 Direction { get; protected set; }



	public new abstract class Builder : Action.Builder {

		public abstract override JumpAction Build(Entity entity);
	}
}