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

	public static byte[] ToByteArray(this Half[] halfs) {
		byte[] bytes = new byte[halfs.Length * 2];
		for (int i = 0; i < halfs.Length; i++) {
			byte[] halfBytes = BitConverter.GetBytes(halfs[i]);

			int j = i * 2;
			bytes[j] = halfBytes[0];
			bytes[j+1] = halfBytes[1];
		}

		return bytes;
	}
	public static byte[] ToByteArray(this float[] floats) {
		byte[] bytes = new byte[floats.Length * 4];
		for (int i = 0; i < floats.Length; i++) {
			byte[] floatBytes = BitConverter.GetBytes(floats[i]);

			int j = i * 4;
			bytes[j] = floatBytes[0];
			bytes[j+1] = floatBytes[1];
			bytes[j+2] = floatBytes[2];
			bytes[j+3] = floatBytes[3];
		}

		return bytes;
	}
	public static byte[] ToByteArray(this double[] doubles) {
		byte[] bytes = new byte[doubles.Length * 8];
		for (int i = 0; i < doubles.Length; i++) {
			byte[] doubleBytes = BitConverter.GetBytes(doubles[i]);

			int j = i * 8;
			bytes[j] = doubleBytes[0];
			bytes[j+1] = doubleBytes[1];
			bytes[j+2] = doubleBytes[2];
			bytes[j+3] = doubleBytes[3];
			bytes[j+4] = doubleBytes[4];
			bytes[j+5] = doubleBytes[5];
			bytes[j+6] = doubleBytes[6];
			bytes[j+7] = doubleBytes[7];
		}

		return bytes;
	}


	public static byte[] ToByteArray(this ushort[] ushorts) {
		byte[] bytes = new byte[ushorts.Length * 2];
		for (int i = 0; i < ushorts.Length; i++) {
			byte[] ushortBytes = BitConverter.GetBytes(ushorts[i]);

			int j = i * 2;
			bytes[j] = ushortBytes[0];
			bytes[j+1] = ushortBytes[1];
		}

		return bytes;
	}
	public static byte[] ToByteArray(this short[] shorts) {
		byte[] bytes = new byte[shorts.Length * 2];
		for (int i = 0; i < shorts.Length; i++) {
			byte[] shortBytes = BitConverter.GetBytes(shorts[i]);

			int j = i * 2;
			bytes[j] = shortBytes[0];
			bytes[j+1] = shortBytes[1];
		}

		return bytes;
	}

	public static byte[] ToByteArray(this uint[] uints) {
		byte[] bytes = new byte[uints.Length * 4];
		for (int i = 0; i < uints.Length; i++) {
			byte[] uintBytes = BitConverter.GetBytes(uints[i]);

			int j = i * 4;
			bytes[j] = uintBytes[0];
			bytes[j+1] = uintBytes[1];
			bytes[j+2] = uintBytes[2];
			bytes[j+3] = uintBytes[3];
		}

		return bytes;
	}
	public static byte[] ToByteArray(this int[] ints) {
		byte[] bytes = new byte[ints.Length * 4];
		for (int i = 0; i < ints.Length; i++) {
			byte[] intBytes = BitConverter.GetBytes(ints[i]);

			int j = i * 4;
			bytes[j] = intBytes[0];
			bytes[j+1] = intBytes[1];
			bytes[j+2] = intBytes[2];
			bytes[j+3] = intBytes[3];
		}

		return bytes;
	}

	public static byte[] ToByteArray(this ulong[] ulongs) {
		byte[] bytes = new byte[ulongs.Length * 8];
		for (int i = 0; i < ulongs.Length; i++) {
			byte[] ulongBytes = BitConverter.GetBytes(ulongs[i]);

			int j = i * 8;
			bytes[j] = ulongBytes[0];
			bytes[j+1] = ulongBytes[1];
			bytes[j+2] = ulongBytes[2];
			bytes[j+3] = ulongBytes[3];
			bytes[j+4] = ulongBytes[4];
			bytes[j+5] = ulongBytes[5];
			bytes[j+6] = ulongBytes[6];
			bytes[j+7] = ulongBytes[7];
		}

		return bytes;
	}
	public static byte[] ToByteArray(this long[] longs) {
		byte[] bytes = new byte[longs.Length * 8];
		for (int i = 0; i < longs.Length; i++) {
			byte[] longBytes = BitConverter.GetBytes(longs[i]);

			int j = i * 8;
			bytes[j] = longBytes[0];
			bytes[j+1] = longBytes[1];
			bytes[j+2] = longBytes[2];
			bytes[j+3] = longBytes[3];
			bytes[j+4] = longBytes[4];
			bytes[j+5] = longBytes[5];
			bytes[j+6] = longBytes[6];
			bytes[j+7] = longBytes[7];
		}

		return bytes;
	}


	public static double Deg2Rad(double degrees) => degrees * (Math.PI / 180.0);
	public static double Rad2Deg(double radians) => radians * (180.0 / Math.PI);
	public static float Deg2Rad(float degrees) => degrees * (Mathf.Pi / 180f);
	public static float Rad2Deg(float radians) => radians * (180f / Mathf.Pi);
}