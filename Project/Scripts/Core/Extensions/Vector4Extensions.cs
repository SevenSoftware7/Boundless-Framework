namespace SevenDev.Utility;

using System;
using Godot;

public static class Vector4Extensions {
	public static Vector4 MoveToward(this Vector4 current, Vector4 target, float maxDistanceDelta) {
		Vector4 vector4 = target - current;
		float magnitude = vector4.Length();
		if (magnitude <= maxDistanceDelta || magnitude == 0.0f) {
			return target;
		}
		return current + vector4 / magnitude * maxDistanceDelta;
	}

	public static Vector4 ClampMagnitude(this Vector4 vector3, float maxLength) {
		if (vector3.LengthSquared() > maxLength * maxLength) {
			return vector3.Normalized() * maxLength;
		}
		return vector3;
	}

	public static Vector4 To(this Vector4 vector, Vector4 to)
		=> new(to.X - vector.X, to.Y - vector.Y, to.Z - vector.Z, to.W - vector.W);

	public static Vector4 SmoothDamp(this Vector4 current, Vector4 target, ref Vector4 currentVelocity, float smoothTime, float maxSpeed, float deltaTime) {
		smoothTime = Math.Max(0.0001f, smoothTime);
		float num1 = 2.0f / smoothTime;
		float num2 = num1 * deltaTime;
		float num3 = 1.0f / (1.0f + num2 + 0.479999989271164f * num2 * num2 + 0.234999999403954f * num2 * num2 * num2);
		Vector4 vector4_1 = current - target;
		Vector4 vector4_2 = target;
		float max = maxSpeed * smoothTime;
		Vector4 vector4_3 = vector4_1.ClampMagnitude(max);
		target = current - vector4_3;
		Vector4 vector4_4 = (currentVelocity + num1 * vector4_3) * deltaTime;
		currentVelocity = (currentVelocity - num1 * vector4_4) * num3;
		Vector4 vector4_5 = target + (vector4_3 + vector4_4) * num3;
		if ((vector4_2 - current).Dot(vector4_5 - vector4_2) > 0.0) {
			vector4_5 = vector4_2;
			currentVelocity = (vector4_5 - vector4_2) / deltaTime;
		}
		return vector4_5;
	}
}