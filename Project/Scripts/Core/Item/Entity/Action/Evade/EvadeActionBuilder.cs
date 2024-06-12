namespace LandlessSkies.Core;

public sealed class EvadeActionBuilder(EvadeActionInfo Info) : EntityActionBuilder {
	protected internal override EvadeAction Build(Entity entity) => Info.Create(entity);
}