namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;
using static SevenDev.Utility.Collisions;

// [Tool]
[GlobalClass]
public partial class CameraController3D : Camera3D {

	[ExportGroup("Options")]
	[Export] private Vector3 cameraOriginPosition;

	[Export] private float horizontalSmoothTime = 0.02f;
	[Export] private float verticalSmoothTime = 0.04f;
	[Export(PropertyHint.Layers3DPhysics)] private uint CollisionMask = uint.MaxValue;
	[ExportGroup("")]


	[Export] public CameraStyle Style = CameraStyle.ThirdPersonGrounded;
	[Export] public Transform3D Subject { get; private set; }

	[Export] public Basis LocalRotation = Basis.Identity;


	private float distanceToSubject = -1f;

	private Vector3 smoothHorizontalPosition = Vector3.Zero;
	private Vector3 smoothVerticalPosition = Vector3.Zero;

	private Vector3 verticalVelocity = Vector3.Zero;
	private Vector3 horizontalVelocity = Vector3.Zero;
	private float distanceVelocity;

	private float verticalTime;



	public void SetEntityAsSubject(Entity entity) {
		Transform3D transform = entity.Transform;
		Vector3 origin = transform.Origin;

		if (entity.Skeleton is not null && entity.Skeleton.TryGetBonePosition("Head", out var bonePosition)) {
			origin = bonePosition;
		}

		Subject = transform with {
			Basis = Subject.Basis * BasisExtensions.FromToBasis(Subject.Basis.Y, entity.UpDirection),
			Origin = origin
		};
	}

	public void MoveCamera(Vector2 cameraInput) {
		Input.MouseMode = Input.MouseModeEnum.Captured;

		if (Style == CameraStyle.ThirdPersonGrounded) {
			float maxAngle = Mathf.Pi / 2f - Mathf.Epsilon;

			Vector3 eulerAngles = LocalRotation.GetEuler();
			LocalRotation = Basis.FromEuler(new(
				Mathf.Clamp(eulerAngles.X + cameraInput.Y, -maxAngle, maxAngle),
				eulerAngles.Y - cameraInput.X,
				0
			));
		}
		else if (Style == CameraStyle.ThirdPerson) {
			LocalRotation *=
				new Basis(LocalRotation.Inverse().Y, -cameraInput.X) *
				new Basis(Vector3.Right, cameraInput.Y);
		}
	}

	public void RawInputToGroundedMovement(Entity entity, Vector2 moveInput, out Basis camRotation, out Vector3 groundedMovement) {
		Vector3 camRight = Subject.Basis * LocalRotation.X;
		float localAlignment = Mathf.Ceil(entity.Transform.Basis.Y.Dot(LocalRotation.Y));
		Vector3 entityUp = entity.Transform.Basis.Y * (localAlignment * 2f - 1f);
		Vector3 groundedCamForward = entityUp.Cross(camRight).Normalized();

		camRotation = Basis.LookingAt(groundedCamForward, entityUp);

		groundedMovement = camRotation * new Vector3(moveInput.X, 0, moveInput.Y).ClampMagnitude(1f);
	}

	public void RawInputToCameraRelativeMovement(Vector2 moveInput, out Basis camRotation, out Vector3 cameraRelativeMovement) {
		camRotation = Subject.Basis * LocalRotation;

		cameraRelativeMovement = camRotation * new Vector3(moveInput.X, 0, moveInput.Y);
	}



	private void ComputeCamera(double delta) {
		float floatDelta = (float)delta;

		Vector3 smoothTargetPosition = GetSmoothTargetPosition(floatDelta);

		// Vector3 targetSubjectSpaceOffset = new(cameraOriginPosition.X, cameraOriginPosition.Y, cameraOriginPosition.Z * distanceToPlayer);
		// subjectSpaceOffset = subjectSpaceOffset.Slerp(targetSubjectSpaceOffset, 3f * floatDelta);


		Basis TargetBasis = Subject.Basis * LocalRotation;

		float targetDistance = cameraOriginPosition.Length();
		Vector3 absoluteOffset = TargetBasis * (cameraOriginPosition / targetDistance);

		ComputeWallCollision(smoothTargetPosition, absoluteOffset, targetDistance, ref distanceToSubject, delta);


		Vector3 finalPos = smoothTargetPosition + absoluteOffset * distanceToSubject;
		GlobalTransform = new(TargetBasis, finalPos);
	}

	private Vector3 GetSmoothTargetPosition(float floatDelta) {
		Vector3 verticalPos = Subject.Origin.Project(Subject.Basis.Y);

		if (! smoothVerticalPosition.IsEqualApprox(verticalPos)) {
			if (Style == CameraStyle.ThirdPersonGrounded) {
				// The camera's new vertical speed is based on the camera's current vertical velocity
				// The camera's vertical movement gets faster as the player keeps moving vertically
				float targetVerticalTime = Mathf.Lerp(verticalSmoothTime, horizontalSmoothTime, Mathf.Clamp(verticalVelocity.LengthSquared(), 0f, 1f));

				float transitionSpeed = targetVerticalTime > verticalTime ? 1.5f : 0.5f; // Accelerate faster than decelerate

				verticalTime = Mathf.Lerp(verticalTime, targetVerticalTime, transitionSpeed * floatDelta);
			}
			else {
				verticalTime = horizontalSmoothTime;
			}
			smoothVerticalPosition = smoothVerticalPosition.SmoothDamp(verticalPos, ref verticalVelocity, verticalTime, Mathf.Inf, floatDelta);
		}

		// Make The Camera Movement slower on the Y axis than on the X axis
		Vector3 horizontalPos = Subject.Origin - verticalPos;
		if (! smoothHorizontalPosition.IsEqualApprox(horizontalPos)) {
			smoothHorizontalPosition = smoothHorizontalPosition.SmoothDamp(horizontalPos, ref horizontalVelocity, horizontalSmoothTime, Mathf.Inf, floatDelta);
		}

		return smoothHorizontalPosition + smoothVerticalPosition;
	}

	private void ComputeWallCollision(Vector3 origin, Vector3 direction, float distance, ref float cameraDistance, double delta) {
		float floatDelta = (float)delta;

		// Check for collision with the camera

		const float CAM_MIN_DISTANCE_TO_WALL = 0.3f;

		SphereShape3D shape = new() {
			Radius = CAM_MIN_DISTANCE_TO_WALL
		};

		PhysicsShapeQueryParameters3D castParameters = new() {
			Transform = new(Basis.Identity, origin),
			Motion = direction * distance,
			Shape = shape,
			CollisionMask = CollisionMask,
		};

		bool rayCastHit = GetWorld3D().CastMotion(castParameters, out CastMotionResult result );


		if (! rayCastHit) {
			if (! Mathf.IsEqualApprox(cameraDistance, distance)) {
				cameraDistance = cameraDistance.SmoothDamp(distance, ref distanceVelocity, 0.2f, Mathf.Inf, floatDelta);
			}

			return;
		}

		cameraDistance = distance * result.SafeProportion;
	}



	public override void _Process(double delta) {
		base._Process(delta);

		if (Engine.IsEditorHint()) return;

		ComputeCamera(delta);
	}

	public override void _Ready() {
		base._Ready();
		LocalRotation = GlobalBasis;

		distanceToSubject = cameraOriginPosition.Length();
		verticalTime = horizontalSmoothTime;
	}



	public enum CameraStyle {
		ThirdPerson,
		ThirdPersonGrounded,
		Fixed
	}
}