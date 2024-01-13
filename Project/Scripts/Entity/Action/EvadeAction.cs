using Godot;

namespace LandlessSkies.Core;

public abstract class EvadeAction : EntityAction {
	public abstract Vector3 Direction { get; set; }
	public abstract float Progress { get; }



	public new interface IInfo : EntityAction.IInfo {
		public new EvadeAction Build();
		EntityAction EntityAction.IInfo.Build() => Build();
	}
}