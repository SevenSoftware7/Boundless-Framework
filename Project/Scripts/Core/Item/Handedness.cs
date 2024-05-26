namespace LandlessSkies.Core;

public enum Handedness : byte {
	Right = 0,
	Left = 1,

}

public static class HandednessExtensions {
	public static Handedness Reverse(this Handedness handedness) => handedness switch {
		Handedness.Left => Handedness.Right,
		Handedness.Right or _ => Handedness.Left
	};
}