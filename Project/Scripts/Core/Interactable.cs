namespace LandlessSkies.Core;

using System.Collections.Generic;
using System.Linq;
using Godot;
using SevenGame.Utility;


public abstract partial class Interactable : Area3D {
	public abstract string InteractLabel { get; protected set; }

	public Interactable() : base() {
		CollisionLayer = MathUtility.InteractableCollisionLayer;
		CollisionMask = 0;
	}

	public abstract bool IsInteractable(Entity entity);
	public abstract void Interact(Entity entity);


	public static Interactable? GetNearestCandidate(Entity entity, float maxDistance, float maxAngle) {
		SphereShape3D sphere = new() {
			Radius = maxDistance,
		};
		float maxDistanceSquared = maxDistance * maxDistance;

		bool anyCandidates = MathUtility.IntersectShape3D(entity.GetWorld3D(), entity.GlobalTransform, Vector3.Zero, out var collisions, sphere, MathUtility.InteractableCollisionLayer);

		IEnumerable<Node3D> buffer = collisions.Select(collision => collision.Collider);

		var (candidate, distance, angle) = buffer.Aggregate((Interactable: null as Interactable, DistanceSquared: float.PositiveInfinity, Angle: float.NegativeInfinity), (best, i) => {
			if (i is not Interactable interactable) return best;

			Vector3 direction = interactable.GlobalPosition.DirectionTo(entity.GlobalPosition);
			float distanceSquared = interactable.GlobalPosition.DistanceSquaredTo(entity.GlobalPosition);
			float angle = direction.Dot(-entity.AbsoluteForward);

			if ( distanceSquared > maxDistanceSquared || angle < maxAngle ) return best;
			if ( distanceSquared > best.DistanceSquared || angle < best.Angle ) return best;

			return (interactable, distanceSquared, angle);
		});


		return candidate;
	}
}
