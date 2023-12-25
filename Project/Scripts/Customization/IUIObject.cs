using Godot;

namespace LandlessSkies.Core;

public interface IUIObject {
	string DisplayName { get; }
	Texture2D? DisplayPortrait { get; }
}