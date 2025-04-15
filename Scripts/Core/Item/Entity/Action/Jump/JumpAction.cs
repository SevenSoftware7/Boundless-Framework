namespace LandlessSkies.Core;

using Godot;

public abstract partial class JumpAction(Entity entity) : EntityAction(entity) {
	public Vector3 Direction { get; set; }



	public new abstract class Builder : EntityAction.Builder {

		public abstract override JumpAction Build(Entity entity);
	}
}