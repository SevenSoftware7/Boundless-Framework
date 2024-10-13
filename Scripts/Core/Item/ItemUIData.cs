namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public partial class ItemUIData : Resource {
	[Export] public virtual string DisplayName { get; private set; } = string.Empty;
	[Export] public virtual Texture2D? DisplayPortrait { get; private set; } = null;
}