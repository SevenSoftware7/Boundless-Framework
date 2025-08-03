namespace Seven.Boundless;

using System;
using Godot;
using Seven.Boundless.Utility;

[Tool]
[GlobalClass]
public sealed partial class DamageHitboxBuilder : Resource {
	[Export] public CollisionShape Shape = CollisionShape.Box;
	[Export] public Vector3 Size = Vector3.One;

	[Export] public DamageHitboxBehaviour? Behaviour = null;


	public CollisionShape3D Build(DamageArea damageArea) {
		CollisionShape3D collision = new() {
			Shape = Shape switch {
				CollisionShape.Box => new BoxShape3D() { Size = Size },
				CollisionShape.Sphere => new SphereShape3D() { Radius = Mathf.Max(Mathf.Max(Size.X, Size.Y), Size.Z) * 0.5f },
				CollisionShape.Capsule => new CapsuleShape3D() { Radius = Mathf.Max(Size.X, Size.Z) * 0.5f, Height = Size.Y },
				CollisionShape.Cylinder => new CylinderShape3D() { Radius = Mathf.Max(Size.X, Size.Z) * 0.5f, Height = Size.Y },
				_ => throw new ArgumentOutOfRangeException(nameof(Shape), Shape, "Invalid collision shape specified.")
			}
		};
		collision.ParentTo(damageArea);

		// MeshInstance3D meshInstance = new() {
		// 	Mesh = Shape switch {
		// 		CollisionShape.Box => new BoxMesh() { Size = Size },
		// 		CollisionShape.Sphere => new SphereMesh() { Radius = Mathf.Max(Mathf.Max(Size.X, Size.Y), Size.Z) * 0.5f, Height = Mathf.Max(Mathf.Max(Size.X, Size.Y), Size.Z) },
		// 		CollisionShape.Capsule => new CapsuleMesh() { Radius = Mathf.Max(Size.X, Size.Z) * 0.5f, Height = Size.Y },
		// 		CollisionShape.Cylinder => new CylinderMesh() { TopRadius = Mathf.Max(Size.X, Size.Z) * 0.5f, BottomRadius = Mathf.Max(Size.X, Size.Z) * 0.5f, Height = Size.Y },
		// 		_ => throw new ArgumentOutOfRangeException(nameof(Shape), Shape, "Invalid collision shape specified.")
		// 	}
		// };
		// meshInstance.ParentTo(collision);


		Behaviour?.Setup(damageArea, collision);

		return collision;
	}

	[Serializable]
	public enum CollisionShape {
		Box,
		Sphere,
		Capsule,
		Cylinder
	}
}