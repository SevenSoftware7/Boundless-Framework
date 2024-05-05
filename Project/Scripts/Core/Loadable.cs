using Godot;

namespace LandlessSkies.Core;

[Tool]
public abstract partial class Loadable : ExtendedNode, ILoadable {
	[Export] public abstract bool IsLoaded { get; set; }
	[Export] public virtual bool IsEnabled {
		get => ProcessMode != ProcessModeEnum.Disabled;
		set {
			if (this.IsInitializationSetterCall())
				return;

			AsIEnablable().EnableDisable(value);
		}
	}

	[Signal] public delegate void LoadedUnloadedEventHandler(bool isLoaded);



	public ILoadable AsILoadable() => this;
	public IEnablable AsIEnablable() => this;

	bool ILoadable.LoadBehaviour() {
		if (!IsInsideTree() || GetParent() is null || Owner is null)
			return false;

		if (!LoadBehaviour())
			return false;

		EmitSignal(SignalName.LoadedUnloaded, true);
		return true;
	}
	protected virtual bool LoadBehaviour() => true;

	void ILoadable.UnloadBehaviour() {
		EmitSignal(SignalName.LoadedUnloaded, false);
		UnloadBehaviour();
	}
	protected virtual void UnloadBehaviour() { }

	void IEnablable.EnableBehaviour() {
		ProcessMode = ProcessModeEnum.Inherit;
		EnableBehaviour();
	}
	protected virtual void EnableBehaviour() { }

	void IEnablable.DisableBehaviour() {
		ProcessMode = ProcessModeEnum.Disabled;
		DisableBehaviour();
	}
	protected virtual void DisableBehaviour() { }



	public override void _EnterTree() {
		base._EnterTree();
		if (IsNodeReady())
			Callable.From(AsILoadable().Load).CallDeferred();
	}
	public override void _ExitTree() {
		base._ExitTree();
		Callable.From(AsILoadable().Unload).CallDeferred();
	}
}