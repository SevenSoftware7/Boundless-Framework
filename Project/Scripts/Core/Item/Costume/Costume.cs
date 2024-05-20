namespace LandlessSkies.Core;

using Godot;


[Tool]
[GlobalClass]
public abstract partial class Costume : Resource, IUIObject {
	public abstract string DisplayName { get; protected set; }
	public abstract Texture2D? DisplayPortrait { get; protected set; }


	public abstract Model? Instantiate();
}