namespace SevenGame.Utility;

using Godot;

public struct Vector2Info {
	public Vector2 currentValue = Vector2.Zero;
	public Vector2 lastValue = Vector2.Zero;

	public Timer zeroTimer = new();
	public Timer nonZeroTimer = new();

	public readonly float X => currentValue.X;
	public readonly float Y => currentValue.Y;



	public Vector2Info() { }



	public readonly float LengthSquared() => currentValue.LengthSquared();
	public readonly float Length() => currentValue.Length();
	public readonly Vector2 Normalized() => currentValue.Normalized();


	public void SetVal(Vector2 value) {
		currentValue = value;

		if (LengthSquared() == 0) {
			nonZeroTimer.Start();
		} else {
			zeroTimer.Start();
		}
	}

	public static implicit operator Vector2(Vector2Info data) => data.currentValue;
	public static Vector2 operator *(Vector2Info a, float b) => a.currentValue * b;
	public static Vector2 operator *(float a, Vector2Info b) => a * b.currentValue;
}