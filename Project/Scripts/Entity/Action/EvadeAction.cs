using Godot;

namespace LandlessSkies.Core;

public abstract class EvadeAction : EntityAction {
	public abstract Vector3 Direction { get; set; }
	public abstract float Progress { get; }



	public interface IEvadeInfo : IInfo {
		public new EvadeAction Build();
		EntityAction IInfo.Build() => Build();
	}
}