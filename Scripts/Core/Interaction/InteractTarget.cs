namespace LandlessSkies.Core;

using System.Linq;
using Godot;
using SevenDev.Boundless.Utility;
using System.Collections.Generic;
using static SevenDev.Boundless.Utility.Collisions;

public struct InteractTarget(Interactable interactable, int shapeIndex) {
	public readonly Interactable Interactable => interactable;
	public readonly int ShapeIndex => shapeIndex;

	public static InteractTarget? GetBestTarget(Entity entity, float maxDistance) {
		return InteractCandidate.GetNearCandidates(entity, maxDistance)?
			.OrderByDescending(x => x.Incidence)
			.ThenBy(x => x.DistanceSquared)
			.Select(x => x.Target)
			.FirstOrDefault();
	}


	private struct InteractCandidate(InteractTarget target, float distanceSquared, float incidence) {
		public readonly InteractTarget Target => target;
		public readonly float DistanceSquared => distanceSquared;
		public readonly float Incidence => incidence;


		public static IEnumerable<InteractCandidate> GetNearCandidates(Entity entity, float maxDistance) {
			SphereShape3D sphere = new() {
				Radius = maxDistance,
			};
			Collisions.IntersectShape3D(entity.GetWorld3D(), entity.GlobalTransform, out IntersectShape3DResult[] collisions, sphere, CollisionLayers.Interactable);

			return GetCandidates(collisions, entity, maxDistance);
		}
		public static IEnumerable<InteractCandidate> GetCandidates(IEnumerable<IntersectShape3DResult> collisions, Entity entity, float maxDistance) {
			float maxDistanceSquared = maxDistance * maxDistance;
			const float leniancyRangeSquared = 0.5f * 0.5f;

			IEnumerable<InteractCandidate> res = collisions
				// .AsParallel()
				.Where(x => x.Collider is Interactable)
				.Select(x => {
					Interactable interactable = (x.Collider as Interactable)!;

					// Get all the necessary info to sort the Interactables
					Transform3D collisionShape = PhysicsServer3D.AreaGetShapeTransform(x.Rid, x.Shape);
					Transform3D shapeTransform = interactable.GlobalTransform * collisionShape;

					float distanceSquared = entity.GlobalPosition.To(shapeTransform.Origin).Slide(entity.UpDirection).LengthSquared();

					Vector3 direction = entity.GlobalPosition.To(interactable.GlobalPosition).Slide(entity.UpDirection).Normalized();
					float incidence = direction.Dot(entity.GlobalForward);

					return new InteractCandidate(new InteractTarget(interactable, x.Shape), distanceSquared, incidence);
				})
				.Where(x => x.DistanceSquared < maxDistanceSquared && x.Target.Interactable.MinLookIncidence is null || x.Incidence > x.Target.Interactable.MinLookIncidence || x.DistanceSquared < leniancyRangeSquared);

			return res;
		}
	}
}