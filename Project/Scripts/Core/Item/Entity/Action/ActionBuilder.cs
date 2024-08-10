namespace LandlessSkies.Core;

/// <summary>
/// 	An EntityActionBuilder is what instantiates, sets up and starts an EntityAction.
/// 	<para>It is passed to <see cref="Entity.ExecuteAction(ActionBuilder, bool)"/> to execute the given Action.</para>
/// </summary>
public abstract class ActionBuilder {
	public System.Action? BeforeExecute { get; set; }
	public System.Action? AfterExecute { get; set; }

	/// <summary>
	/// Instantiates and sets up the Action, using the given Entity as the Target of the Action.
	/// </summary>
	/// <param name="entity">The Entity which will execute the Action</param>
	/// <returns></returns>
	protected internal abstract Action Create(Entity entity);

	internal Action Build(Entity entity) {
		Action action = Create(entity);
		action.OnStart += BeforeExecute;
		action.OnStop += AfterExecute;

		return action;
	}
}