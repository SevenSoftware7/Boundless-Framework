global using InteractTarget = (LandlessSkies.Core.Interactable interactable, Godot.Transform3D shapeTransform);

namespace LandlessSkies.Core;

using System.Linq;
using Godot;
using SevenDev.Utility;


public abstract partial class Interactable : Area3D {
	public abstract string InteractLabel { get; protected set; }

	public Interactable() : base() {
		CollisionLayer = MathUtility.InteractableCollisionLayer;
		CollisionMask = 0;
	}

	public abstract bool IsInteractable(Entity entity);
	public abstract void Interact(Entity entity);


	public static InteractTarget? GetNearestCandidate(Entity entity, float maxDistance, float minIncidence) {
		SphereShape3D sphere = new() {
			Radius = maxDistance,
		};
		float maxDistanceSquared = maxDistance * maxDistance;

		bool anyCandidates = MathUtility.IntersectShape3D(entity.GetWorld3D(), entity.GlobalTransform, out MathUtility.IntersectShape3DResult[] collisions, sphere, MathUtility.InteractableCollisionLayer);

		var (target, distanceSquared, incidence) = collisions.Aggregate((Target: new InteractTarget?(), DistanceSquared: float.PositiveInfinity, Incidence: float.NegativeInfinity), (best, i) => {
			if (i.Collider is not Interactable interactable) return best;
			Transform3D collisionShape = PhysicsServer3D.AreaGetShapeTransform(i.Rid, i.Shape);
			Transform3D shapeTransform = interactable.GlobalTransform * collisionShape;

			Vector3 direction = entity.GlobalPosition.DirectionToUnormalized(shapeTransform.Origin).Slide(entity.UpDirection).Normalized();
			float distanceSquared = entity.GlobalPosition.DistanceSquaredTo(shapeTransform.Origin);
			float incidence = direction.Dot(entity.AbsoluteForward);

			GD.Print(incidence);

			if ( distanceSquared > maxDistanceSquared || incidence < minIncidence && distanceSquared < 0.125f ) return best;
			if ( distanceSquared > best.DistanceSquared || incidence < best.Incidence ) return best;

			return ((interactable, shapeTransform), distanceSquared, incidence);
		});


		return target;
	}
}
