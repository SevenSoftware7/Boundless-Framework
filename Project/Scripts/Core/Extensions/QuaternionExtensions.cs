namespace SevenDev.Utility;

using Godot;

public static class QuaternionExtensions {
	public static Quaternion SafeSlerp(this Quaternion from, Quaternion to, float weight) {
		if (from.IsEqualApprox(to)) {
			return from;
		}
		return from.Slerp(to, weight);
	}
	public static Quaternion FromToQuaternion(this Vector3 from, Vector3 to) {
		Vector3 cross = from.Cross(to);
		float dot = from.Dot(to);
		float w = Mathf.Sqrt(from.LengthSquared() * to.LengthSquared()) + dot;
		return new Quaternion(cross.X, cross.Y, cross.Z, w);
	}

	public static Quaternion FromToQuaternion(this Quaternion from, Quaternion to) {
		return to * from.Inverse();
	}
}