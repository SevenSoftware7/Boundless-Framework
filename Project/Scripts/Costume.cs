using Godot;

namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public abstract partial class Costume : Resource, IUIObject {
	public abstract string DisplayName { get; }
	public abstract Texture2D? DisplayPortrait { get; }


	public abstract Model Instantiate();
}