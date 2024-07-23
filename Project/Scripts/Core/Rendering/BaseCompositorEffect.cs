namespace LandlessSkies.Core;

using Godot;
using Godot.Collections;

public abstract partial class BaseCompositorEffect : CompositorEffect, ISerializationListener {
	protected RenderingDevice? RenderingDevice { get; private set; }



	public BaseCompositorEffect() : base() {
		RenderingDevice ??= RenderingServer.GetRenderingDevice();
		Construct();
	}

	public override void _Notification(int what) {
		base._Notification(what);
		switch ((ulong)what) {
			case NotificationPredelete:
				Destruct();
				break;
		}
	}


	protected void Construct() {
		if (RenderingDevice is null) return;

		RenderingServer.CallOnRenderThread(Callable.From(() => ConstructBehaviour(RenderingDevice)));
	}
	protected void Destruct() {
		if (RenderingDevice is null) return;

		DestructBehaviour(RenderingDevice);
	}

	protected abstract void ConstructBehaviour(RenderingDevice renderingDevice);
	protected abstract void DestructBehaviour(RenderingDevice renderingDevice);

	public void OnBeforeSerialize() {
		Destruct();
	}
	public void OnAfterDeserialize() {
		Construct();
	}

	public override void _ValidateProperty(Dictionary property) {
		base._ValidateProperty(property);

		StringName name = property["name"].AsStringName();


		if (name == PropertyName.RenderingDevice) {
			property["usage"] = (int)(property["usage"].As<PropertyUsageFlags>() & ~PropertyUsageFlags.Storage);
		}
	}
}