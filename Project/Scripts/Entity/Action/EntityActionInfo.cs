using System;
using Godot;

namespace LandlessSkies.Core;


public abstract record class EntityActionInfo() {
	public Action? BeforeExecute { get; set; }
	public Action? AfterExecute { get; set; }


	public abstract EntityAction Build();

	public EntityAction Execute() {
		BeforeExecute?.Invoke();
		EntityAction action = Build();
		action.OnDestroy += AfterExecute;

		return action;
	}
}