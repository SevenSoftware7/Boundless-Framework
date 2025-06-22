namespace SevenDev.Boundless;

using Godot;
using Godot.Collections;

[GlobalClass]
public abstract partial class PromptControl : Control {
	public enum PromptHideDirection {
		Left,
		Top,
		Right,
		Bottom
	};

	[Export] public bool Enabled;
	[Export] public PromptHideDirection direction = PromptHideDirection.Left;

	public void SetEnabled(bool enabled) {
		Enabled = enabled;
	}

	public abstract void SetText(string text);
	public abstract void SetKey(Texture2D image);


	public void Update(bool enabled, string text) {
		SetEnabled(enabled);

		SetText(text);
	}
	public void Update(bool enabled, Texture2D key) {
		SetEnabled(enabled);

		SetKey(key);
	}
	public void Update(bool enabled, string text, Texture2D key) {
		SetEnabled(enabled);

		SetText(text);
		SetKey(key);
	}

	public void Update(InteractTarget? interactTarget) {
		if (!interactTarget.HasValue) {
			SetEnabled(false);
			return;
		}

		SetEnabled(true);
		SetText(interactTarget.Value.Interactable.InteractLabel);
	}

	public virtual void Destroy() { } // TODO: When CancelFree works (again), replace with the standard QueueFree pipeline


	public override void _ValidateProperty(Dictionary property) {
		base._ValidateProperty(property);

		StringName name = property["name"].AsStringName();


		if (name == PropertyName.Enabled) {
			property["usage"] = (long)(property["usage"].As<PropertyUsageFlags>() & ~PropertyUsageFlags.Storage);
		}
	}
}
