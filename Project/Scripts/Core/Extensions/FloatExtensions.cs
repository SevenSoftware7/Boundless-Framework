using Godot;

namespace LandlessSkies.Core;

public static class FloatExtensions {
	public static float Clamp01(this float value) {
		return Mathf.Clamp(value, 0f, 1f);
	}
}