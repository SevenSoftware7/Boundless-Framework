namespace Seven.Boundless;

using Godot;

public abstract partial class EvadeAction : EntityAction {
	public abstract Vector3 Direction { get; set; }
	public abstract float Progress { get; }


	public EvadeAction(Entity entity, Builder builder) : base(entity, builder) { }


	public new abstract class Builder() : EntityAction.Builder {
		protected internal abstract EvadeAction Create(Entity entity);
	}
}