using Godot;

namespace SevenDev.Utility;

public static class AreaExtensions {
	public static bool GetSurfaceAbove(this Area3D area, Vector3 location, float distance, out Collisions.IntersectRay3DResult result) {
		return area.GetWorld3D().IntersectRay3DExclusive(area, location + Vector3.Up * distance, location, out result, area.CollisionLayer);
	}
}