namespace Seven.Boundless;

using System;
using Godot;
using Godot.Collections;

public abstract partial class BaseCompositorEffect : CompositorEffect, ISerializationListener {
	protected RenderingDevice RenderingDevice { get; init; }



	public BaseCompositorEffect() : base() {
		RenderingDevice ??= RenderingServer.GetRenderingDevice();
		if (RenderingDevice is null) {
			Enabled = false;
			throw new PlatformNotSupportedException("Compositor Effects not supported on the current Rendering Driver");
		}
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

		RenderingServer.CallOnRenderThread(Callable.From(ConstructBehaviour));
	}
	protected void Destruct() {
		if (RenderingDevice is null) return;

		DestructBehaviour();
	}

	protected abstract void ConstructBehaviour();
	protected abstract void DestructBehaviour();

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
			property["usage"] = (long)(property["usage"].As<PropertyUsageFlags>() & ~PropertyUsageFlags.Storage);
		}
	}
}