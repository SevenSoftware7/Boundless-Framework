using System;
using Godot;
using Godot.Collections;

namespace SevenGame.Utility;

public static class MathUtility {
	public static Vector4 MoveToward(this Vector4 current, Vector4 target, float maxDistanceDelta) {
		Vector4 vector4 = target - current;
		float magnitude = vector4.Length();
		if (magnitude <= maxDistanceDelta || magnitude == 0.0f) {
			return target;
		}
		return current + vector4 / magnitude * maxDistanceDelta;
	}

	public static Vector2 SafeSlerp(this Vector2 from, Vector2 to, float weight) {
		if (from.IsEqualApprox(to)) {
			return from;
		}
		return from.Slerp(to, weight);
	}
	public static Vector3 SafeSlerp(this Vector3 from, Vector3 to, float weight) {
		if (from.IsEqualApprox(to)) {
			return from;
		}
		return from.Slerp(to, weight);
	}
	public static Quaternion SafeSlerp(this Quaternion from, Quaternion to, float weight) {
		if (from.IsEqualApprox(to)) {
			return from;
		}
		return from.Slerp(to, weight);
	}
	public static Basis SafeSlerp(this Basis from, Basis to, float weight) {
		if (from.IsEqualApprox(to)) {
			return from;
		}
		return from.Orthonormalized().Slerp(to.Orthonormalized(), weight);
	}


	public static Vector2 ClampMagnitude(this Vector2 vector2, float maxLength) {
		if (vector2.LengthSquared() > maxLength * maxLength) {
			return vector2.Normalized() * maxLength;
		}
		return vector2;
	}
	public static Vector3 ClampMagnitude(this Vector3 vector3, float maxLength) {
		if (vector3.LengthSquared() > maxLength * maxLength) {
			return vector3.Normalized() * maxLength;
		}
		return vector3;
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

	public static Basis FromToBasis(this Vector3 from, Vector3 to) {
		return new(FromToQuaternion(from, to));
	}

	public static Basis FromToBasis(this Basis from, Basis to) {
		return to * from.Inverse();
	}

	/// <summary>
	/// Returns the target vector after being "flattened" against a surface defined by the given normal.
	/// </summary>
	/// <param name="vector"></param>
	/// <param name="direction"></param>
	/// <returns></returns>
	public static Vector3 FlattenInDirection( this Vector3 vector, Vector3 direction ) {
		return vector - direction * Mathf.Max(vector.Project(direction).Dot(direction), 0);
	}

	public static double SmoothDamp(double current, double target, ref double currentVelocity, double smoothTime, double maxSpeed, double deltaTime) {
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
		if (num5 - current > 0.0 == num8 > num5) {
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
		if (num5 - current > 0.0f == num8 > num5) {
			num8 = num5;
			currentVelocity = (num8 - num5) / deltaTime;
		}
		return num8;
	}

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

	public static bool RayCast3D(this Node3D node, Vector3 from, Vector3 to, out RayCast3DResult result, uint collisionMask = uint.MaxValue, Array<Rid>? exclude = null, bool collideWithBodies = true, bool collideWithAreas = true, bool hitFromInside = false, bool hitBackFaces = false) {
		PhysicsDirectSpaceState3D spaceState = node.GetWorld3D().DirectSpaceState;
		PhysicsRayQueryParameters3D parameters = PhysicsRayQueryParameters3D.Create(from, to, collisionMask, exclude);
		parameters.CollideWithBodies = collideWithBodies;
		parameters.CollideWithAreas = collideWithAreas;
		parameters.HitFromInside = hitFromInside;
		parameters.HitBackFaces = hitBackFaces;
		
		Dictionary intersect = spaceState.IntersectRay(parameters);
		if ( intersect.Count > 0 ) {
			result = new RayCast3DResult() {
				HasHit = true,
				Point = intersect["position"].AsVector3(),
				Normal = intersect["normal"].AsVector3(),
				Collider = intersect["collider"].AsGodotObject(),
				Id = intersect["collider_id"].AsUInt64(),
				Rid = intersect["rid"].AsRid(),
				Shape = intersect["shape"].AsInt32(),
				// Metadata = intersect["metadata"] // Can't seem to get this to work
			};
			return true;
		}

		result = new() {
			HasHit = false
		};
		return false;
	}

	public struct RayCast3DResult {
		public bool HasHit;
		public Vector3 Point;
		public Vector3 Normal;
		public GodotObject Collider;
		public ulong Id;
		public Rid Rid;
		public int Shape;
		// public Variant Metadata;
	}


	public static double Deg2Rad(double degrees) => degrees * (Math.PI / 180.0);
	public static double Rad2Deg(double radians) => radians * (180.0 / Math.PI);
	public static float Deg2Rad(float degrees) => degrees * (Mathf.Pi / 180f);
	public static float Rad2Deg(float radians) => radians * (180f / Mathf.Pi);
}