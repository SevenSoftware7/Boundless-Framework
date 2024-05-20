namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;


[Tool]
public abstract partial class Model : Node3D, IEnablable {
	[Export] public virtual bool IsEnabled {
		get => ProcessMode != ProcessModeEnum.Disabled;
		set {
			if (this.IsInitializationSetterCall()) return;

			AsIEnablable().EnableDisable(value);
		}
	}

	public IEnablable AsIEnablable() => this;



	protected Model() : base() { }

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

	public abstract Aabb GetAabb();
}