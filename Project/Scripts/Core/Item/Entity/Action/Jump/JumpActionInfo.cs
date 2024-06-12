namespace LandlessSkies.Core;

using Godot;

public abstract class JumpActionInfo() {
	/// <summary>
	/// The Proportion of the Entity's Jump Height Stat that this Action can travel
	/// </summary>
	public virtual float PotentialHeight { get; }

	protected internal abstract JumpAction Create(Entity entity, Vector3 direction);
}