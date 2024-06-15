namespace LandlessSkies.Core;

using System.Linq;
using Godot;
using SevenDev.Utility;
using System.Collections.Generic;
using System;
using static SevenDev.Utility.Collisions;

public abstract partial class Interactable : Area3D {
	public abstract string InteractLabel { get; }
	public virtual float? MinLookIncidence => null;


	public Interactable() : base() {
		CollisionLayer = Collisions.Interactable;
		CollisionMask = 0;
	}


	public CollisionShape3D? GetShape3D(int shapeIndex) {
		return ShapeOwnerGetOwner(ShapeFindOwner(shapeIndex)) as CollisionShape3D;
	}


	public abstract bool IsInteractable(Entity entity);
	public abstract void Interact(Entity entity, Player? player = null, int shapeIndex = 0);
}


public sealed class InteractTarget(Interactable interactable, int shapeIndex) : Tuple<Interactable, int>(interactable, shapeIndex) {
	public Interactable Interactable => Item1;
	public int ShapeIndex => Item2;

	public static InteractTarget? GetBestTarget(Entity entity, float maxDistance) {
		return InteractCandidate.GetNearCandidates(entity, maxDistance)?
			.OrderBy(x => x.DistanceSquared)
			.ThenByDescending(x => x.Incidence)
			.Select(x => x.Target)
			.FirstOrDefault();
	}
}


public sealed class InteractCandidate(InteractTarget target, float distanceSquared, float incidence) {
	public InteractTarget Target => target;
	public float DistanceSquared => distanceSquared;
	public float Incidence => incidence;


	public static IEnumerable<InteractCandidate> GetNearCandidates(Entity entity, float maxDistance) {
		SphereShape3D sphere = new() {
			Radius = maxDistance,
		};
		Collisions.IntersectShape3D(entity.GetWorld3D(), entity.GlobalTransform, out IntersectShape3DResult[] collisions, sphere, Collisions.Interactable);

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