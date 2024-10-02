namespace LandlessSkies.Core;

using Godot;

public abstract partial class EvadeAction(Entity entity) : Action(entity) {
	public abstract Vector3 Direction { get; set; }
	public abstract float Progress { get; }



	public new abstract class Builder() : Action.Builder {
		protected internal abstract EvadeAction Create(Entity entity);
	}
}