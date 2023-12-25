using Godot;

namespace LandlessSkies.Core;

public abstract class EvadeAction : EntityAction {
	public abstract Vector3 Direction { get; set; }
	public abstract float Progress { get; }



	public new abstract class Info() : EntityAction.Info() {
		protected abstract override EvadeAction Build();
	}
}