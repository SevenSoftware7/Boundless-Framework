namespace LandlessSkies.Core;

using Godot;
using SevenGame.Utility;

[Tool]
[GlobalClass]
public partial class CameraController3D : Camera3D {

	[ExportGroup("Options")]
	[Export] private Vector3 cameraOriginPosition;

	[Export] private float horizontalSmoothTime = 0.02f;
	[Export] private float verticalSmoothTime = 0.04f;
	[Export(PropertyHint.Layers3DPhysics)] private uint CollisionMask = uint.MaxValue;
	[ExportGroup("")]


	[Export] public CameraStyle Style = CameraStyle.ThirdPersonGrounded;
	[Export] public Basis SubjectBasis = Basis.Identity;
	[Export] public Vector3 Subject;

	[Export] public Basis LocalRotation { get; private set; } = Basis.Identity;


	private float distanceToSubject = -1f;

	private Vector3 smoothHorizontalPosition = Vector3.Zero;
	private Vector3 smoothVerticalPosition = Vector3.Zero;

	private Vector3 verticalVelocity = Vector3.Zero;
	private Vector3 horizontalVelocity = Vector3.Zero;
	private float distanceVelocity = 0f;

	private float verticalTime = 0f;


	public Basis AbsoluteRotation => SubjectBasis * LocalRotation;



	public void SetEntityAsSubject(Entity entity) {
		Subject = entity.Skeleton?.GetBonePositionOrDefault("Head", Subject) ?? entity.Transform.Origin;

		SubjectBasis = entity.Transform.Basis;
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
		} else if (Style == CameraStyle.ThirdPerson) {
			LocalRotation *=
				new Basis(LocalRotation.Inverse().Y, -cameraInput.X) *
				new Basis(Vector3.Right, cameraInput.Y);
		}
	}

	public void RawInputToGroundedMovement(Entity entity, Vector2 moveInput, out Basis camRotation, out Vector3 groundedMovement) {
		Vector3 camRight = AbsoluteRotation.X;
		float localAlignment = Mathf.Ceil(entity.Transform.Basis.Y.Dot(LocalRotation.Y));
		Vector3 entityUp = entity.Transform.Basis.Y * (localAlignment * 2f - 1f);
		Vector3 groundedCamForward = entityUp.Cross(camRight).Normalized();

		camRotation = Basis.LookingAt(groundedCamForward, entityUp);

		groundedMovement = camRotation * new Vector3(moveInput.X, 0, moveInput.Y).ClampMagnitude(1f);
	}

	public void RawInputToCameraRelativeMovement(Vector2 moveInput, out Basis camRotation, out Vector3 cameraRelativeMovement) {
		camRotation = AbsoluteRotation;

		cameraRelativeMovement = camRotation * new Vector3(moveInput.X, 0, moveInput.Y);
	}


	private void ComputeCamera(double delta) {
		float floatDelta = (float) delta;

		Vector3 smoothTargetPosition = GetSmoothTargetPosition(floatDelta);


		// Vector3 targetSubjectSpaceOffset = new(cameraOriginPosition.X, cameraOriginPosition.Y, cameraOriginPosition.Z * distanceToPlayer);
		// subjectSpaceOffset = subjectSpaceOffset.Slerp(targetSubjectSpaceOffset, 3f * floatDelta);


		Basis TargetBasis = AbsoluteRotation;

		float targetDistance = cameraOriginPosition.Length();
		Vector3 absoluteOffset = TargetBasis * (cameraOriginPosition / targetDistance);

		ComputeWallCollision(smoothTargetPosition, absoluteOffset, targetDistance, ref distanceToSubject, delta);


		Vector3 finalPos = smoothTargetPosition + absoluteOffset * distanceToSubject;
		GlobalTransform = new(TargetBasis, finalPos);
	}

	private Vector3 GetSmoothTargetPosition(float floatDelta) {
		Vector3 verticalPos = Subject.Project(SubjectBasis.Y);

		if (!smoothVerticalPosition.IsEqualApprox(verticalPos)) {
			if (Style == CameraStyle.ThirdPersonGrounded) {
				// The camera's new vertical speed is based on the camera's current vertical velocity
				// The camera's vertical movement gets faster as the player keeps moving vertically

				float targetVerticalTime = Mathf.Lerp(verticalSmoothTime, horizontalSmoothTime, Mathf.Clamp(verticalVelocity.LengthSquared(), 0f, 1f));

				float transitionSpeed = targetVerticalTime > verticalTime ? 1.5f : 0.5f; // Accelerate faster than decelerate

				verticalTime = Mathf.Lerp(verticalTime, targetVerticalTime, transitionSpeed * floatDelta);
			} else {
				verticalTime = horizontalSmoothTime;
			}
			smoothVerticalPosition = smoothVerticalPosition.SmoothDamp(verticalPos, ref verticalVelocity, verticalTime, Mathf.Inf, floatDelta);
		}

		// Make The Camera Movement slower on the Y axis than on the X axis

		Vector3 horizontalPos = Subject - verticalPos;
		if (!smoothHorizontalPosition.IsEqualApprox(horizontalPos)) {
			smoothHorizontalPosition = smoothHorizontalPosition.SmoothDamp(horizontalPos, ref horizontalVelocity, horizontalSmoothTime, Mathf.Inf, floatDelta);
		}


		Vector3 smoothPosition = smoothHorizontalPosition + smoothVerticalPosition;
		return smoothPosition;
	}

	private void ComputeWallCollision(Vector3 origin, Vector3 direction, float distance, ref float cameraDistance, double delta) {
		float floatDelta = (float) delta;

		// Check for collision with the camera

		const float CAM_MIN_DISTANCE_TO_WALL = 0.4f;

		bool rayCastHit = GetWorld3D().RayCast3D(origin, origin + direction * (distance + CAM_MIN_DISTANCE_TO_WALL), out MathUtility.RayCast3DResult result, CollisionMask);
		if (!rayCastHit) {
			if (!Mathf.IsEqualApprox(cameraDistance, distance)) {
				cameraDistance = cameraDistance.SmoothDamp(distance, ref distanceVelocity, 0.2f, Mathf.Inf, floatDelta);
			}

			return;
		}


		Vector3 collisionToPlayer = origin - result.Point;
		float collisionDistance = collisionToPlayer.Length();
		collisionToPlayer /= collisionDistance; // cheaper than Normalize, since we already have the length

		// Fancy Trigonometry to keep the camera at least distanceToWall away from the wall
		// this will keep the camera in the same direction as it would have been without collision,
		// but at a constant distance from the wall
		//
		// |                                          [Camera]
		// |                                        /--
		// |                                     /--
		// |                                  /--
		// |                               /--
		// |                            /--
		// |  Collision point        [*]--> Final position
		// |        ^             /-- |
		// |        |          /--    |
		// |        |       /--       +---> length of CAM_MIN_DISTANCE_TO_WALL
		// |        |    /--          |
		// |        | /--             |
		// +-------[*]----------------+------------------------------------------

		float angle = collisionToPlayer.AngleTo(result.Normal);
		float collisionAngle = (Mathf.Pi / 2f) - angle;
		float camMargin = CAM_MIN_DISTANCE_TO_WALL / Mathf.Sin(collisionAngle);

		cameraDistance = collisionDistance - camMargin;
	}



	public override void _Process(double delta) {
		base._Process(delta);

		if (Engine.IsEditorHint())
			return;

		ComputeCamera(delta);
	}

	public override void _EnterTree() {
		base._EnterTree();
		distanceToSubject = cameraOriginPosition.Length();
		verticalTime = horizontalSmoothTime;
	}



	public enum CameraStyle {
		ThirdPerson,
		ThirdPersonGrounded,
		Fixed
	}
}