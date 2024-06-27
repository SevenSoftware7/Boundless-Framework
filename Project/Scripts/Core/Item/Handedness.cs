namespace LandlessSkies.Core;

public enum Handedness : byte {
	Right,
	Left,
}

public static class HandednessExtensions {
	public static Handedness Reverse(this Handedness handedness) => handedness switch {
		Handedness.Left       => Handedness.Right,
		Handedness.Right or _ => Handedness.Left,
	};
}