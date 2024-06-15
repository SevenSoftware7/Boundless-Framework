namespace LandlessSkies.Core;

using System;

public abstract class EntityActionBuilder {
	public Action? BeforeExecute { get; set; }
	public Action? AfterExecute { get; set; }

	protected internal abstract EntityAction Create(Entity entity);

	public EntityAction Build(Entity entity) {
		EntityAction action = Create(entity);
		action.OnStart += BeforeExecute;
		action.OnStop += AfterExecute;

		return action;
	}
}