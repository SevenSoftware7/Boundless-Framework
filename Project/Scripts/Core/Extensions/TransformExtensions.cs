namespace SevenDev.Utility;

using Godot;

public static class TransformExtensions {
	public static Vector3 Right(this Transform3D transform) => - transform.Basis.X;
	public static Vector3 Up(this Transform3D transform) => transform.Basis.Y;
	public static Vector3 Forward(this Transform3D transform) => - transform.Basis.Z;
	public static Vector3 Left(this Transform3D transform) => - transform.Right();
	public static Vector3 Down(this Transform3D transform) => - transform.Up();
	public static Vector3 Back(this Transform3D transform) => - transform.Forward();
}