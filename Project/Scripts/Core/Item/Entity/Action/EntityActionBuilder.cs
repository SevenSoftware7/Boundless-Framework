namespace LandlessSkies.Core;

using System;

public abstract class EntityActionBuilder {
	public Action? BeforeExecute { get; set; }
	public Action? AfterExecute { get; set; }

	protected internal abstract EntityAction Build(Entity entity);

	public EntityAction Execute(Entity entity) {
		BeforeExecute?.Invoke();
		EntityAction action = Build(entity);
		action.OnDestroy += AfterExecute;

		return action;
	}
}