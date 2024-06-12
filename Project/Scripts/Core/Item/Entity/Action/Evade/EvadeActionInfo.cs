namespace LandlessSkies.Core;

public abstract class EvadeActionInfo() {
	public float PotentialDistance { get; }

	protected internal abstract EvadeAction Create(Entity entity);
}