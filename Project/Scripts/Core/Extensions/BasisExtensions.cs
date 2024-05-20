namespace SevenDev.Utility;

using Godot;

public static class BasisExtensions {
	public static Vector3 Right(this Basis basis) => - basis.X;
	public static Vector3 Up(this Basis basis) => basis.Y;
	public static Vector3 Forward(this Basis basis) => - basis.Z;
	public static Vector3 Left(this Basis basis) => - basis.Right();
	public static Vector3 Down(this Basis basis) => - basis.Up();
	public static Vector3 Back(this Basis basis) => - basis.Forward();

	public static Basis SafeSlerp(this Basis from, Basis to, float weight) {
		if (from.IsEqualApprox(to)) {
			return from;
		}
		return from.Orthonormalized().Slerp(to.Orthonormalized(), weight);
	}

	public static Basis FromToBasis(this Vector3 from, Vector3 to) {
		return new(QuaternionExtensions.FromToQuaternion(from, to));
	}

	public static Basis FromToBasis(this Basis from, Basis to) {
		return to * from.Inverse();
	}
}