namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public partial class HudPack : Resource {
	[Export] public PackedScene? InteractPrompt { get; private set; }
	[Export] public PackedScene? InteractPointer { get; private set; }
}
