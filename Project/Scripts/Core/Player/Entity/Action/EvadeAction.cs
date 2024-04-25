namespace LandlessSkies.Core;

using Godot;

public abstract partial class EvadeAction : EntityAction {
	public abstract Vector3 Direction { get; set; }
	public abstract float Progress { get; }
}