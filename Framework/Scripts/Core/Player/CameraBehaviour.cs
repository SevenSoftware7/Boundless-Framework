namespace SevenDev.Boundless;

using System;
using Godot;
using SevenDev.Boundless.Utility;

public abstract partial class CameraBehaviour : Behaviour<CameraBehaviour> {
	[Export(PropertyHint.Layers3DPhysics)] private uint CollisionMask = uint.MaxValue & ~(CollisionLayers.Water | CollisionLayers.Entity | CollisionLayers.Prop | CollisionLayers.Interactable | CollisionLayers.Damage);
	private float distanceVelocity;


	public readonly CameraController3D CameraController;



	public CameraBehaviour(CameraController3D camera) : base() {
		ArgumentNullException.ThrowIfNull(camera, nameof(camera));
		CameraController = camera;
	}


	// Check for collision with the camera
	protected void ComputeWallCollision(Vector3 origin, Vector3 direction, float distance, ref float cameraDistance, float delta) {
		const float CAM_MIN_DISTANCE_TO_WALL = 0.3f;

		PhysicsShapeQueryParameters3D castParameters = new() {
			Transform = new(Basis.Identity, origin),
			Motion = direction * distance,
			Shape = new SphereShape3D() { Radius = CAM_MIN_DISTANCE_TO_WALL },
			CollisionMask = CollisionMask,
		};

		bool rayCastHit = CameraController.GetWorld3D().CastMotion(castParameters, out Collisions.CastMotionResult result);


		if (!rayCastHit) {
			if (!Mathf.IsEqualApprox(cameraDistance, distance)) {
				cameraDistance = cameraDistance.SmoothDamp(distance, ref distanceVelocity, 0.2f, Mathf.Inf, delta);
			}

			return;
		}

		cameraDistance = distance * result.SafeProportion;
	}
}