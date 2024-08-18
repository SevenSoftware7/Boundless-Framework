namespace LandlessSkies.Core;

using Godot;

public abstract partial class EvadeAction(Entity entity) : Action(entity) {
	public abstract Vector3 Direction { get; set; }
	public abstract float Progress { get; }



	public abstract class Builder() {
		public float PotentialDistance { get; }

		protected internal abstract EvadeAction Create(Entity entity);
	}

	public new sealed class Wrapper(Builder Info) : Action.Wrapper {
		protected internal override EvadeAction Create(Entity entity) => Info.Create(entity);
	}
}