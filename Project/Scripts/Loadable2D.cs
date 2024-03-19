using Godot;

namespace LandlessSkies.Core;

[Tool]
public abstract partial class Loadable2D : ExtendedNode2D, ILoadable {
	[Export] public abstract bool IsLoaded { get; set; }
	[Signal] public delegate void LoadedUnloadedEventHandler(bool isLoaded);



	public ILoadable AsILoadable() => this;

	bool ILoadable.LoadModelBehaviour() {
		if (!IsInsideTree() || GetParent() is null || Owner is null)
			return false;

		if (!LoadModelBehaviour())
			return false;

		EmitSignal(SignalName.LoadedUnloaded, true);
		return true;
	}
	void ILoadable.UnloadModelBehaviour() {
		UnloadModelBehaviour();
		EmitSignal(SignalName.LoadedUnloaded, false);
	}

	protected virtual bool LoadModelBehaviour() => true;
	protected virtual void UnloadModelBehaviour() { }

	public virtual void Enable() {
		ProcessMode = ProcessModeEnum.Inherit;
		Visible = true;
	}
	public virtual void Disable() {
		ProcessMode = ProcessModeEnum.Disabled;
		Visible = false;
	}
	public virtual void Destroy() {
		AsILoadable().UnloadModel();
		this.UnparentAndQueueFree();
	}



	public override void _EnterTree() {
		base._EnterTree();
		if (IsNodeReady())
			Callable.From(AsILoadable().LoadModel).CallDeferred();
	}
	public override void _ExitTree() {
		base._ExitTree();
		Callable.From(AsILoadable().UnloadModel).CallDeferred();
	}
}