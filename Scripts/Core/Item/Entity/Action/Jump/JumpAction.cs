namespace Seven.Boundless;

using Godot;

public abstract partial class JumpAction : EntityAction {
	public Vector3 Direction { get; set; }


	public JumpAction(Entity entity, Builder builder) : base(entity, builder) { }


	public new abstract class Builder : EntityAction.Builder {

		public abstract override JumpAction Build(Entity entity);
	}
}