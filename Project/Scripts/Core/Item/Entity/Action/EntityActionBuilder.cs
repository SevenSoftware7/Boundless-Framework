namespace LandlessSkies.Core;

using System;

/// <summary>
/// 	An EntityActionBuilder is what instantiates, sets up and starts an EntityAction.
/// 	<para>It is passed to <see cref="Entity.ExecuteAction(EntityActionBuilder, bool)"/> to execute the given Action.</para>
/// </summary>
public abstract class EntityActionBuilder {
	public Action? BeforeExecute { get; set; }
	public Action? AfterExecute { get; set; }

	/// <summary>
	/// Instantiates and sets up the Action, using the given Entity as the Target of the Action.
	/// </summary>
	/// <param name="entity">The Entity which will execute the Action</param>
	/// <returns></returns>
	protected internal abstract EntityAction Create(Entity entity);

	internal EntityAction Build(Entity entity) {
		EntityAction action = Create(entity);
		action.OnStart += BeforeExecute;
		action.OnStop += AfterExecute;

		return action;
	}
}