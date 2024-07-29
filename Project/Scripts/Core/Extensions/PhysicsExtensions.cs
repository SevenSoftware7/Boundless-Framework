namespace SevenDev.Utility;

using Godot;

public static class PhysicsExtensions {
	public static bool GetSurfaceInDirection(this Area3D area, Vector3 location, Vector3 direction, out Collisions.IntersectRay3DResult result) {
		if (direction == Vector3.Zero) direction = Vector3.Up;
		direction = direction.IsNormalized() ? direction : direction.Normalized();

		Aabb aabb = new(area.GlobalPosition, Vector3.One * 200f); // FIXME: get area AABB when possible

		float depth = Mathf.Abs((aabb.Size * 0.5f).Dot(direction));

		Vector3 inside = location + (area.GlobalPosition - location).SlideOnFace(-direction).Project(-direction);
		Vector3 outside = location + direction * (depth + Mathf.Epsilon);

		return area.GetWorld3D().IntersectRay3DExclusive(area, outside, inside, out result, area.CollisionLayer, collideWithBodies: false);
	}
}