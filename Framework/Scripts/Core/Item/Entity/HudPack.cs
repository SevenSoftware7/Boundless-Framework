namespace Seven.Boundless;

using Godot;

[Tool]
[GlobalClass]
public partial class HudPack : Resource {
	[Export] public PackedScene? HealthBar { get; private set; }
	[Export] public PackedScene? InteractPrompt { get; private set; }
	[Export] public PackedScene? InteractPointer { get; private set; }
	[Export] public PackedScene? ToggleButton { get; private set; }
	[Export] public PackedScene? OptionList { get; private set; }
	[Export] public PackedScene? OptionButton { get; private set; }
}
