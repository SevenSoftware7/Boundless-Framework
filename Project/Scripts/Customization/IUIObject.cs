namespace LandlessSkies.Core;

using Godot;

public interface IUIObject {
	string DisplayName { get; }
	Texture2D? DisplayPortrait { get; }
}