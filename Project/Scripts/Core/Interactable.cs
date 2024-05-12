global using InteractTarget = (LandlessSkies.Core.Interactable interactable, Godot.Transform3D shapeTransform);

namespace LandlessSkies.Core;

using System.Linq;
using Godot;
using SevenGame.Utility;
using static SevenGame.Utility.MathUtility;


public abstract partial class Interactable : Area3D {
	public abstract string InteractLabel { get; protected set; }

	public Interactable() : base() {
		CollisionLayer = MathUtility.InteractableCollisionLayer;
		CollisionMask = 0;
	}

	public abstract bool IsInteractable(Entity entity);
	public abstract void Interact(Entity entity);


	public static InteractTarget? GetNearestCandidate(Entity entity, float maxDistance, float maxAngle) {
		SphereShape3D sphere = new() {
			Radius = maxDistance,
		};
		float maxDistanceSquared = maxDistance * maxDistance;

		bool anyCandidates = MathUtility.IntersectShape3D(entity.GetWorld3D(), entity.GlobalTransform, Vector3.Zero, out IntersectShape3DResult[] collisions, sphere, MathUtility.InteractableCollisionLayer);

		var (target, shapeTransform, distance, angle) = collisions.Aggregate((Target: null as Interactable, Shape: Transform3D.Identity, DistanceSquared: float.PositiveInfinity, Angle: float.NegativeInfinity), (best, i) => {
			if (i.Collider is not Interactable interactable) return best;
			Transform3D collisionShape = PhysicsServer3D.AreaGetShapeTransform(i.Rid, i.Shape);
			// Vector3 shapePosition = interactable.ToGlobal(collisionShape.Origin);
			Transform3D shapeTransform = interactable.GlobalTransform * collisionShape;

			Vector3 direction = shapeTransform.Origin.DirectionTo(entity.GlobalPosition);
			float distanceSquared = shapeTransform.Origin.DistanceSquaredTo(entity.GlobalPosition);
			float angle = direction.Dot(-entity.AbsoluteForward);

			if ( distanceSquared > maxDistanceSquared || angle < maxAngle ) return best;
			if ( distanceSquared > best.DistanceSquared || angle < best.Angle ) return best;

			return (interactable, shapeTransform, distanceSquared, angle);
		});


		return target is null ? null : (target, shapeTransform);
	}
}
