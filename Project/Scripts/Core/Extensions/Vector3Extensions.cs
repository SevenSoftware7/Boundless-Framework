namespace SevenDev.Utility;

using System;
using Godot;

public static class Vector3Extensions {
	public static Vector3 SafeSlerp(this Vector3 from, Vector3 to, float weight) {
		if (!from.IsFinite() || !to.IsFinite()) return from;
		if (from.IsEqualApprox(to)) return from;

		// Avoid error on both vectors being inverses of each other, breaking a Cross Product operation in the Slerp method
		if ((from + to).IsEqualApprox(Vector3.Zero)) {
			return from.Lerp(to, weight);
		}


		return from.Slerp(to, weight);
	}

	public static Vector3 ClampMagnitude(this Vector3 vector3, float maxLength) {
		if (vector3.LengthSquared() > maxLength * maxLength) {
			return vector3.Normalized() * maxLength;
		}
		return vector3;
	}

	public static Vector3 ClampedLerp(this Vector3 from, Vector3 to, float amount) {
		Vector3 res = from + (to - from) * amount;
		if (amount > Mathf.Epsilon && from.IsEqualApprox(res)) {
			return to;
		}
		return res;
	}

	/// <summary>
	/// Returns the target vector after being "flattened" against a surface defined by the given normal.
	/// </summary>
	/// <param name="vector"></param>
	/// <param name="direction"></param>
	/// <returns></returns>
	public static Vector3 SlideOnFace(this Vector3 vector, Vector3 normal) =>
		vector + normal * Mathf.Max(vector.Project(-normal).Dot(-normal), 0);

	public static Vector3 To(this Vector3 vector, Vector3 to)
		=> new(to.X - vector.X, to.Y - vector.Y, to.Z - vector.Z);

	public static Vector3 SmoothDamp(this Vector3 current, Vector3 target, ref Vector3 currentVelocity, float smoothTime, float maxSpeed, float deltaTime) {
		smoothTime = Math.Max(0.0001f, smoothTime);
		float num1 = 2.0f / smoothTime;
		float num2 = num1 * deltaTime;
		float num3 = 1.0f / (1.0f + num2 + 0.479999989271164f * num2 * num2 + 0.234999999403954f * num2 * num2 * num2);
		Vector3 vector3_1 = current - target;
		Vector3 vector3_2 = target;
		float max = maxSpeed * smoothTime;
		Vector3 vector3_3 = vector3_1.ClampMagnitude(max);
		target = current - vector3_3;
		Vector3 vector3_4 = (currentVelocity + num1 * vector3_3) * deltaTime;
		currentVelocity = (currentVelocity - num1 * vector3_4) * num3;
		Vector3 vector3_5 = target + (vector3_3 + vector3_4) * num3;
		if ((vector3_2 - current).Dot(vector3_5 - vector3_2) > 0.0) {
			vector3_5 = vector3_2;
			currentVelocity = (vector3_5 - vector3_2) / deltaTime;
		}
		return vector3_5;
	}


	public static void Split(this Vector3 vector3, Vector3 upDirection, out Vector3 vertical, out Vector3 horizontal) {
		vertical = vector3.Project(upDirection);
		horizontal = vector3 - vertical;
	}
}