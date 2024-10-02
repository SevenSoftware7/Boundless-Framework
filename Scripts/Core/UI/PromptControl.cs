namespace LandlessSkies.Core;

using Godot;

[GlobalClass]
public abstract partial class PromptControl : Control {
	[Export] public bool Enabled;


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
		if (interactTarget is null) {
			SetEnabled(false);
			return;
		}

		SetEnabled(true);
		SetText(interactTarget.Interactable.InteractLabel);
	}

	public virtual void Destroy() { } // TODO: When CancelFree works (again), replace with the standard QueueFree pipeline
}
