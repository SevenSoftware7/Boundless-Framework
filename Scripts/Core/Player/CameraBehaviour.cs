namespace LandlessSkies.Core;

using System;
using Godot;
using SevenDev.Boundless.Utility;

public abstract partial class CameraBehaviour : Behaviour<CameraBehaviour> {
	public abstract Transform3D Transform { get; }
	public abstract Vector3 TargetPosition { get; protected set; }

	[Export(PropertyHint.Layers3DPhysics)] private uint CollisionMask = uint.MaxValue & ~(CollisionLayers.Water | CollisionLayers.Entity | CollisionLayers.Prop | CollisionLayers.Interactable | CollisionLayers.Damage);
	private float distanceVelocity;


	[Export] public CameraController3D CameraController { get; private set; }



	protected CameraBehaviour() : this(null) { }
	public CameraBehaviour(CameraController3D? camera) : base() {
		CameraController = camera!;
	}


	protected override void _Start(CameraBehaviour? previousBehaviour = null) {
		if (CameraController is null) {
			Stop();
			throw new ArgumentNullException($"Could not start Behaviour {GetType().Name}, no reference to a CameraController");
		}

		base._Start(previousBehaviour);
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