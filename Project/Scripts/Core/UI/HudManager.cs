namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;

[GlobalClass]
public partial class HudManager : Control {
	[Export] public Camera3D? ProjectorCamera;

	[Export] public Control PointerContainer { get; private set; } = null!;
	[Export] public Control InfoContainer { get; private set; } = null!;
	[Export] public Control PromptContainer { get; private set; } = null!;


	public HudManager() { }


	public GaugeControl? AddInfo(PackedScene? info) {
		if (info is null)
			return null;

		return AddInfo(info.Instantiate<GaugeControl>());
	}
	public GaugeControl AddInfo(GaugeControl info) {
		info.ParentTo(InfoContainer);
		InfoContainer.MoveChild(info, 0);
		return info;
	}

	public PromptControl AddPrompt(PackedScene prompt) {
		return AddPrompt(prompt.Instantiate<PromptControl>());
	}
	public PromptControl AddPrompt(PromptControl prompt) {
		prompt.ParentTo(PromptContainer);
		PromptContainer.MoveChild(prompt, 0);
		return prompt;
	}

	public PointerControl AddPointer(PackedScene pointer) {
		return AddPointer(pointer.Instantiate<PointerControl>());
	}
	public PointerControl AddPointer(PointerControl prompt) {
		prompt.ProjectorCamera = ProjectorCamera;
		return prompt.ParentTo(PointerContainer);
	}
}