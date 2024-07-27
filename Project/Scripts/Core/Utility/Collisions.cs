namespace SevenDev.Utility;

using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

public static class Collisions {
	public static readonly uint Terrain = 1 << 0;
	public static readonly uint Entity = 1 << 1;
	public static readonly uint Water = 1 << 2;
	public static readonly uint Interactable = 1 << 3;
	public static readonly uint Damage = 1 << 4;


	public static bool IntersectRay3DExclusive<T>(this World3D world, T target, Vector3 from, Vector3 to, out IntersectRay3DResult result, uint collisionMask = uint.MaxValue, Array<Rid>? exclude = null, bool collideWithBodies = true, bool collideWithAreas = true, bool hitFromInside = false, bool hitBackFaces = false) where T : Node3D {
		PhysicsRayQueryParameters3D parameters = PhysicsRayQueryParameters3D.Create(from, to, collisionMask);
		parameters.CollideWithBodies = collideWithBodies;
		parameters.CollideWithAreas = collideWithAreas;
		parameters.HitFromInside = hitFromInside;
		parameters.HitBackFaces = hitBackFaces;

		return world.IntersectRay3DExclusive(target, parameters, out result, exclude);
	}
	public static bool IntersectRay3DExclusive<T>(this World3D world, T target, PhysicsRayQueryParameters3D parameters, out IntersectRay3DResult result, Array<Rid>? exclude = null) where T : Node3D {
		exclude ??= [];
		PhysicsDirectSpaceState3D spaceState = world.DirectSpaceState;

		IntersectRay3DResult? lastSuccessHit = null;

		const int maxRetries = 15;
		for (int i = 0; i < maxRetries; i++) {
			parameters.Exclude = exclude;

			Dictionary intersect = spaceState.IntersectRay(parameters);
			if (!DictionaryToIntersectRay3DResult(intersect, out IntersectRay3DResult rayHit)) {
				result = lastSuccessHit ?? rayHit;
				return lastSuccessHit is not null;
			}

			// Exclude the erroneous collider
			if (rayHit.Collider != target) {
				exclude.Add(rayHit.Rid);
				continue;
			}

			// Store the success
			lastSuccessHit = rayHit;
			// And try to find another one, further behind
			parameters.From = lastSuccessHit.Value.Point;
		}

		result = default;
		return false;
	}

	public static bool IntersectRay3D(this World3D world, Vector3 from, Vector3 to, out IntersectRay3DResult result, uint collisionMask = uint.MaxValue, Array<Rid>? exclude = null, bool collideWithBodies = true, bool collideWithAreas = true, bool hitFromInside = false, bool hitBackFaces = false) {
		PhysicsRayQueryParameters3D parameters = PhysicsRayQueryParameters3D.Create(from, to, collisionMask, exclude);
		parameters.CollideWithBodies = collideWithBodies;
		parameters.CollideWithAreas = collideWithAreas;
		parameters.HitFromInside = hitFromInside;
		parameters.HitBackFaces = hitBackFaces;

		return world.IntersectRay3D(parameters, out result);
	}
	public static bool IntersectRay3D(this World3D world, PhysicsRayQueryParameters3D parameters, out IntersectRay3DResult result) {
		PhysicsDirectSpaceState3D spaceState = world.DirectSpaceState;

		Dictionary intersect = spaceState.IntersectRay(parameters);
		return DictionaryToIntersectRay3DResult(intersect, out result);
	}

	public static bool DictionaryToIntersectRay3DResult(Dictionary intersect, out IntersectRay3DResult result) {
		if (intersect.Count == 0) {
			result = default;
			return false;
		}

		Vector3 normal = intersect["normal"].AsVector3();
		result = new IntersectRay3DResult() {
			Point = intersect["position"].AsVector3(),
			Normal = normal,
			Collider = intersect["collider"].As<Node3D>(),
			Id = intersect["collider_id"].AsUInt64(),
			Rid = intersect["rid"].AsRid(),
			Shape = intersect["shape"].AsInt32(),
			FaceIndex = intersect["face_index"].AsInt32(),
			HitFromInside = normal == Vector3.Zero,
		};
		return true;
	}


	public static bool IntersectPoint3D(this World3D world, Vector3 position, out IntersectShape3DResult[] results, uint collisionMask = uint.MaxValue, Array<Rid>? exclude = null, bool collideWithBodies = true, bool collideWithAreas = true, int maxResults = 32) {
		PhysicsPointQueryParameters3D parameters = new() {
			Position = position,
			Exclude = exclude,
			CollisionMask = collisionMask,
			CollideWithBodies = collideWithBodies,
			CollideWithAreas = collideWithAreas,
		};

		return world.IntersectPoint3D(parameters, out results, maxResults);
	}
	public static bool IntersectPoint3D(this World3D world, PhysicsPointQueryParameters3D parameters, out IntersectShape3DResult[] results, int maxResults = 32) {
		PhysicsDirectSpaceState3D spaceState = world.DirectSpaceState;

		Array<Dictionary> intersections = spaceState.IntersectPoint(parameters, maxResults);
		if (intersections.Count == 0) {
			results = [];
			return false;
		}


		results = intersections.Select(intersection => {
			return new IntersectShape3DResult() {
				Collider = intersection["collider"].As<Node3D>(),
				Id = intersection["collider_id"].AsUInt64(),
				Rid = intersection["rid"].AsRid(),
				Shape = intersection["shape"].AsInt32()
			};
		}).ToArray();

		return results.Length > 0;
	}

	public static bool IntersectShape3D(this World3D world, Transform3D origin, out IntersectShape3DResult[] results, Shape3D shape, uint collisionMask = uint.MaxValue, Array<Rid>? exclude = null, bool collideWithBodies = true, bool collideWithAreas = true, int maxResults = 32) {
		PhysicsShapeQueryParameters3D parameters = new() {
			Transform = origin,
			Exclude = exclude,
			CollisionMask = collisionMask,
			CollideWithBodies = collideWithBodies,
			CollideWithAreas = collideWithAreas,
			Shape = shape,
		};

		return world.IntersectShape3D(parameters, out results, maxResults);
	}
	public static bool IntersectShape3D(this World3D world, PhysicsShapeQueryParameters3D parameters, out IntersectShape3DResult[] results, int maxResults = 32) {
		PhysicsDirectSpaceState3D spaceState = world.DirectSpaceState;

		Array<Dictionary> intersections = spaceState.IntersectShape(parameters, maxResults);
		if (intersections.Count == 0) {
			results = [];
			return false;
		}

		results = intersections.Select(intersection => {
			return new IntersectShape3DResult() {
				Collider = intersection["collider"].As<Node3D>(),
				Id = intersection["collider_id"].AsUInt64(),
				Rid = intersection["rid"].AsRid(),
				Shape = intersection["shape"].AsInt32()
			};
		}).ToArray();

		return results.Length > 0;
	}

	public static bool CollideShape3D(this World3D world, Transform3D origin, out CollideShape3DResult[] results, Shape3D shape, uint collisionMask = uint.MaxValue, Array<Rid>? exclude = null, bool collideWithBodies = true, bool collideWithAreas = true, int maxResults = 32) {
		PhysicsShapeQueryParameters3D parameters = new() {
			Transform = origin,
			Exclude = exclude,
			CollisionMask = collisionMask,
			CollideWithBodies = collideWithBodies,
			CollideWithAreas = collideWithAreas,
			Shape = shape,
		};

		return world.CollideShape3D(parameters, out results, maxResults);
	}
	public static bool CollideShape3D(this World3D world, PhysicsShapeQueryParameters3D parameters, out CollideShape3DResult[] results, int maxResults = 32) {
		PhysicsDirectSpaceState3D spaceState = world.DirectSpaceState;

		Array<Vector3> collisions = spaceState.CollideShape(parameters, maxResults);
		if (collisions.Count == 0) {
			results = [];
			return false;
		}


		List<CollideShape3DResult> resultsList = [];
		for (int i = 0; i < collisions.Count; i += 2) {
			resultsList.Add(new() {
				InShape = collisions[i],
				InWorld = collisions[i + 1]
			});
		}
		results = [.. resultsList];

		return results.Length > 0;
	}


	public static bool CastMotion(this World3D world, Transform3D origin, Vector3 motion, out CastMotionResult result, Shape3D shape, uint collisionMask = uint.MaxValue, Array<Rid>? exclude = null, bool collideWithBodies = true, bool collideWithAreas = true) {
		PhysicsShapeQueryParameters3D parameters = new() {
			Transform = origin,
			Motion = motion,
			Exclude = exclude,
			CollisionMask = collisionMask,
			CollideWithBodies = collideWithBodies,
			CollideWithAreas = collideWithAreas,
			Shape = shape,
		};

		return world.CastMotion(parameters, out result);
	}
	public static bool CastMotion(this World3D world, PhysicsShapeQueryParameters3D parameters, out CastMotionResult result) {
		PhysicsDirectSpaceState3D spaceState = world.DirectSpaceState;

		float[] proportions = spaceState.CastMotion(parameters);
		float safeProportion = proportions[0];
		float unsafeProportion = proportions[1];

		if (proportions.Length != 2 || safeProportion == 1f && unsafeProportion == 1f) {
			result = new();
			return false;
		}


		result = new() {
			SafeProportion = safeProportion,
			UnsafeProportion = unsafeProportion
		};

		return true;
	}


	public struct IntersectRay3DResult {
		public Vector3 Point;
		public Vector3 Normal;
		public Node3D Collider;
		public ulong Id;
		public Rid Rid;
		public int Shape;
		public int FaceIndex;
		public bool HitFromInside;
	}

	public struct IntersectShape3DResult {
		public Node3D Collider;
		public ulong Id;
		public Rid Rid;
		public int Shape;
	}

	public struct CollideShape3DResult {
		public Vector3 InShape;
		public Vector3 InWorld;
	}

	public struct CastMotionResult {
		public float SafeProportion;
		public float UnsafeProportion;
	}
}