namespace SevenDev.Utility;

using System;
using System.Runtime.CompilerServices;
using Godot;

public static class Mathfs {
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Clamp01(this float value) {
		return Mathf.Clamp(value, 0f, 1f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Clamp01(this double value) {
		return Mathf.Clamp(value, 0f, 1f);
	}


	public static double SmoothDamp(this double current, double target, ref double currentVelocity, double smoothTime, double maxSpeed, double deltaTime) {
		smoothTime = Math.Max(0.0001, smoothTime);
		double num1 = 2.0 / smoothTime;
		double num2 = num1 * deltaTime;
		double num3 = 1.0 / (1.0 + num2 + 0.479999989271164 * num2 * num2 + 0.234999999403954 * num2 * num2 * num2);
		double num4 = current - target;
		double num5 = target;
		double max = maxSpeed * smoothTime;
		double num6 = Math.Max(-max, Math.Min(max, num4));
		target = current - num6;
		double num7 = (currentVelocity + num1 * num6) * deltaTime;
		currentVelocity = (currentVelocity - num1 * num7) * num3;
		double num8 = target + (num6 + num7) * num3;
		if ((num5 - current > 0.0) == (num8 > num5)) {
			num8 = num5;
			currentVelocity = (num8 - num5) / deltaTime;
		}
		return num8;
	}

	public static float SmoothDamp(this float current, float target, ref float currentVelocity, float smoothTime, float maxSpeed, float deltaTime) {
		smoothTime = Math.Max(0.0001f, smoothTime);
		float num1 = 2.0f / smoothTime;
		float num2 = num1 * deltaTime;
		float num3 = 1.0f / (1.0f + num2 + 0.479999989271164f * num2 * num2 + 0.234999999403954f * num2 * num2 * num2);
		float num4 = current - target;
		float num5 = target;
		float max = maxSpeed * smoothTime;
		float num6 = Math.Max(-max, Math.Min(max, num4));
		target = current - num6;
		float num7 = (currentVelocity + num1 * num6) * deltaTime;
		currentVelocity = (currentVelocity - num1 * num7) * num3;
		float num8 = target + (num6 + num7) * num3;
		if ((num5 - current > 0.0f) == (num8 > num5)) {
			num8 = num5;
			currentVelocity = (num8 - num5) / deltaTime;
		}
		return num8;
	}


	public static double Deg2Rad(double degrees) => degrees * (Math.PI / 180.0);
	public static double Rad2Deg(double radians) => radians * (180.0 / Math.PI);
	public static float Deg2Rad(float degrees) => degrees * (Mathf.Pi / 180f);
	public static float Rad2Deg(float radians) => radians * (180f / Mathf.Pi);
}