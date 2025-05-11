namespace LandlessSkies.Core;

using Godot;
using SevenDev.Boundless.Utility;

public sealed partial class GravitatedCameraBehaviour : EntityCameraBehaviour {
	public override Vector3 Velocity {
		get => horizontalVelocity + verticalVelocity;
		protected set {
			Vector3 up = Subject?.Basis.Y ?? Vector3.Up;

			verticalVelocity = value.Project(up);
			horizontalVelocity = value - verticalVelocity;
		}
	}

	protected override bool IsOneTime { get; } = false;

	private float _verticalLookAngle = 0f;

	[Export] public float HorizontalSmoothTime = 0.065f;
	[Export] public float VerticalSmoothTime = 0.16f;

	private Vector3 verticalVelocity = Vector3.Zero;
	private Vector3 horizontalVelocity = Vector3.Zero;
	private float verticalTime;



	private GravitatedCameraBehaviour() : this(null!) { }
	public GravitatedCameraBehaviour(CameraController3D camera) : base(camera) { }


	public override void MoveCamera(Vector2 cameraInput) {
		float maxVerticalAngle = Mathf.Pi / 2f - Mathf.Epsilon;

		_verticalLookAngle = Mathf.Clamp(_verticalLookAngle + Mathf.DegToRad(cameraInput.Y), -maxVerticalAngle, maxVerticalAngle);
		LookRotation = new Basis(Vector3.Right, _verticalLookAngle);

		LocalRotation = LocalRotation.Rotated(LocalRotation.Up(), Mathf.DegToRad(-cameraInput.X));
	}


	public override void _Process(double delta) {
		float floatDelta = (float)delta;

		if (Subject is null) return;
		Transform3D subjectTransform = SubjectTransform;

		Basis warpedRotation = LocalRotation.WarpUpTowards(Subject.UpDirection);
		LocalRotation = LocalRotation.SafeSlerp(warpedRotation.Orthonormalized(), 8f * floatDelta);

		FollowPosition.Split(LocalRotation.Up(), out Vector3 smoothVerticalPosition, out Vector3 smoothHorizontalPosition);
		subjectTransform.Origin.Split(LocalRotation.Up(), out Vector3 verticalPos, out Vector3 horizontalPos);


		if (!smoothVerticalPosition.IsEqualApprox(verticalPos)) {
			// The camera's new vertical speed is based on the camera's current vertical velocity
			// The camera's vertical movement gets faster as the player keeps moving vertically
			float targetVerticalTime = Mathf.Lerp(VerticalSmoothTime, HorizontalSmoothTime, Mathf.Clamp(verticalVelocity.LengthSquared(), 0f, 1f));

			float transitionSpeed = targetVerticalTime > verticalTime ? 1.5f : 0.5f; // Accelerate faster than decelerate

			verticalTime = Mathf.Lerp(verticalTime, targetVerticalTime, transitionSpeed * floatDelta);
		}
		smoothVerticalPosition = smoothVerticalPosition.SmoothDamp(verticalPos, ref verticalVelocity, verticalTime, Mathf.Inf, floatDelta);


		// Make The Camera Movement slower on the Y axis than on the X axis
		if (!smoothHorizontalPosition.IsEqualApprox(horizontalPos)) {
			smoothHorizontalPosition = smoothHorizontalPosition.SmoothDamp(horizontalPos, ref horizontalVelocity, HorizontalSmoothTime, Mathf.Inf, floatDelta);
		}

		FollowPosition = smoothVerticalPosition + smoothHorizontalPosition;

		base._Process(delta);
	}
}