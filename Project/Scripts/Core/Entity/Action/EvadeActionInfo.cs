namespace LandlessSkies.Core;

public abstract record class EvadeActionInfo() : EntityActionInfo() {
	public abstract override EvadeAction Build();
}