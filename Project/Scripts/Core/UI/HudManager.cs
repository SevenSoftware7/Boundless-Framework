namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public partial class HudManager : Control {
	[Export] public Camera3D? ProjectorCamera;

	[Export] public Control PromptContainer { get; private set; } = null!;
	[Export] public Control PointerContainer { get; private set; } = null!;


	public HudManager() { }


	public PromptControl AddPrompt(PackedScene prompt) {
		return AddPrompt(prompt.Instantiate<PromptControl>());
	}
	public PromptControl AddPrompt(PromptControl prompt) {
		return prompt.ParentTo(PromptContainer);
	}

	public PointerControl AddPointer(PackedScene prompt) {
		return AddPointer(prompt.Instantiate<PointerControl>());
	}
	public PointerControl AddPointer(PointerControl prompt) {
		prompt.ProjectorCamera = ProjectorCamera;
		return prompt.ParentTo(PointerContainer);
	}
}