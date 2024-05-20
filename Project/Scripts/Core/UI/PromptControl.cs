namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public abstract partial class PromptControl : Control {
	[Export] public bool Enabled;


	public abstract void SetText(string text);
	public abstract void SetKey(Texture2D image);

	public void Update(bool enabled, string text) {
		Enabled = enabled;

		if (Enabled) {
			SetText(text);
		}
	}
	public void Update(bool enabled, string text, Texture2D key) {
		Enabled = enabled;

		if (Enabled) {
			SetText(text);
			SetKey(key);
		}
	}
	public void Update(InteractTarget? interactTarget) {
		Enabled = interactTarget is not null;

		if (interactTarget is not null) {
			SetText(interactTarget.Interactable.InteractLabel);
		}
	}
}
