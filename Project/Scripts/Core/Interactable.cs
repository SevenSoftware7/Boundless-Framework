global using InteractTarget = (LandlessSkies.Core.Interactable interactable, Godot.Transform3D shapeTransform, int shapeIndex);

namespace LandlessSkies.Core;

using System.Linq;
using Godot;
using SevenDev.Utility;
using System.Collections.Generic;
using System;


public abstract partial class Interactable : Area3D {
	public abstract string InteractLabel { get; }

	public Interactable() : base() {
		CollisionLayer = MathUtility.InteractableCollisionLayer;
		CollisionMask = 0;
	}

	public abstract bool IsInteractable(Entity entity);
	public abstract void Interact(Entity entity, int shapeIndex = 0);


	public static InteractTarget? GetNearestCandidate(Entity entity, float maxDistance, float minIncidence) {
		SphereShape3D sphere = new() {
			Radius = maxDistance,
		};
		float maxDistanceSquared = maxDistance * maxDistance;
		const float leniancyRangeSquared = 0.5f * 0.5f;

		bool anyCandidates = MathUtility.IntersectShape3D(entity.GetWorld3D(), entity.GlobalTransform, out MathUtility.IntersectShape3DResult[] collisions, sphere, MathUtility.InteractableCollisionLayer);

		var res = collisions
			// .AsParallel()
			.Where(x => x.Collider is Interactable)
			.Select(x => {
				Interactable interactable = (x.Collider as Interactable)!;

				// Get all the necessary info to sort the Interactables
				Transform3D collisionShape = PhysicsServer3D.AreaGetShapeTransform(x.Rid, x.Shape);
				Transform3D shapeTransform = interactable.GlobalTransform * collisionShape;

				Vector3 flattenedVector = entity.GlobalPosition.DirectionToUnormalized(shapeTransform.Origin).Slide(entity.UpDirection);

				Vector3 direction = flattenedVector.Normalized();
				float distanceSquared = flattenedVector.LengthSquared();
				float incidence = direction.Dot(entity.AbsoluteForward);

				(InteractTarget target, float distanceSquared, float incidence) res = ((interactable, shapeTransform, x.Shape), distanceSquared, incidence);
				return res;
			})
			.Where(x => x.distanceSquared < maxDistanceSquared && x.incidence > minIncidence || x.distanceSquared < leniancyRangeSquared)
			.OrderBy(x => - x.incidence)
			.ThenBy(x => x.distanceSquared)
			.Select(x => x.target);


		return res.Any() ? res.First() : null;
	}


	public CollisionShape3D? GetShape3D(int shapeIndex) {
		return ShapeOwnerGetOwner(ShapeFindOwner(shapeIndex)) as CollisionShape3D;
	}
}
