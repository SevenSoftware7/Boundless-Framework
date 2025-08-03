namespace Seven.Boundless;

using Godot;
using Seven.Boundless.Utility;

[GlobalClass]
public partial class HudManager : Control {
	public enum PromptType {
		Interact,
		Input
	}
	[Export] public Camera3D? ProjectorCamera;

	[Export] public Control PointerContainer { get; private set; } = null!;
	[Export] public Control InfoContainer { get; private set; } = null!;
	[Export] public Control InteractPromptContainer { get; private set; } = null!;
	[Export] public Control InputPromptContainer { get; private set; } = null!;


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

	public PromptControl AddPrompt(PackedScene prompt, PromptType type = PromptType.Interact) {
		return AddPrompt(prompt.Instantiate<PromptControl>(), type);
	}
	public PromptControl AddPrompt(PromptControl prompt, PromptType type = PromptType.Interact) {
		switch (type) {
			default:
			case PromptType.Interact:
				prompt.direction = PromptControl.PromptHideDirection.Left;
				prompt.ParentTo(InteractPromptContainer);
				InteractPromptContainer.MoveChild(prompt, 0);
				break;
			case PromptType.Input:
				prompt.direction = PromptControl.PromptHideDirection.Bottom;
				prompt.ParentTo(InputPromptContainer);
				InputPromptContainer.MoveChild(prompt, 0);
				break;
		}
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