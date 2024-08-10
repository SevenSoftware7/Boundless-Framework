namespace LandlessSkies.Core;

public sealed class EvadeActionBuilder(EvadeActionInfo Info) : ActionBuilder {
	protected internal override EvadeAction Create(Entity entity) => Info.Create(entity);
}