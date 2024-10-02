namespace LandlessSkies.Core;

using Godot;
using SevenDev.Boundless.Utility;

[GlobalClass]
public sealed partial class DamageHitboxBuilder : Resource {
	[Export] public Vector3 Offset = Vector3.Zero;
	[Export] public Vector3 Size = Vector3.One;


	public CollisionShape3D Build(DamageArea damageArea) {
		CollisionShape3D collision = new() {
			Shape = new BoxShape3D() {
				Size = Size,
			}
		};

		new MeshInstance3D() {
			Mesh = new BoxMesh() {
				Size = Size
			}
		}.ParentTo(collision);

		collision.ParentTo(damageArea);
		collision.GlobalTransform = new() {
			Origin = damageArea.GlobalTransform * Offset,
			Basis = damageArea.GlobalBasis
		};

		return collision;
	}
}