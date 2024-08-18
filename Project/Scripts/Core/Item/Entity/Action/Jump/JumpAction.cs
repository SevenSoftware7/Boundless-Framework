namespace LandlessSkies.Core;

using Godot;

public abstract partial class JumpAction(Entity entity) : Action(entity) {
	public Vector3 Direction { get; protected set; }



	public abstract class Builder() {
		/// <summary>
		/// The Proportion of the Entity's Jump Height Stat that this Action can travel
		/// </summary>
		public virtual float PotentialHeight { get; }

		protected internal abstract JumpAction Create(Entity entity, Vector3 direction);
	}

	public new sealed class Wrapper(Builder Info, Vector3 direction) : Action.Wrapper {
		protected internal override JumpAction Create(Entity entity) => Info.Create(entity, direction);
	}
}