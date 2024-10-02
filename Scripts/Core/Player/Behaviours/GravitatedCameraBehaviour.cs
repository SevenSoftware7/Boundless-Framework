namespace LandlessSkies.Core;

using Godot;
using SevenDev.Boundless.Utility;

public sealed partial class GravitatedCameraBehaviour : EntityCameraBehaviour {
	public override Vector3 TargetPosition {
		get => smoothHorizontalPosition + smoothVerticalPosition;
		protected set {
			smoothVerticalPosition = value.Project(Subject.Basis.Y);
			smoothHorizontalPosition = value - smoothVerticalPosition;
		}
	}
	public override Vector3 Velocity {
		get => horizontalVelocity + verticalVelocity;
		protected set {
			verticalVelocity = value.Project(Subject.Basis.Y);
			horizontalVelocity = value - verticalVelocity;
		}
	}

	protected override bool IsOneTime => false;


	[Export] public float HorizontalSmoothTime = 0.065f;
	[Export] public float VerticalSmoothTime = 0.16f;

	private Vector3 smoothVerticalPosition = Vector3.Zero;
	private Vector3 verticalVelocity = Vector3.Zero;
	private float verticalTime;

	private Vector3 smoothHorizontalPosition = Vector3.Zero;
	private Vector3 horizontalVelocity = Vector3.Zero;


	private GravitatedCameraBehaviour() : this(null) { }
	public GravitatedCameraBehaviour(CameraController3D? camera) : base(camera) { }


	public override void MoveCamera(Vector2 cameraInput) {
		float maxAngle = Mathf.Pi / 2f - Mathf.Epsilon;

		Vector3 eulerAngles = LocalRotation.GetEuler();
		LocalRotation = Basis.FromEuler(new(
			Mathf.Clamp(eulerAngles.X + cameraInput.Y, -maxAngle, maxAngle),
			eulerAngles.Y - cameraInput.X,
			0
		));
	}


	public override void _Process(double delta) {
		float floatDelta = (float)delta;

		Vector3 verticalPos = Subject.Origin.Project(Subject.Basis.Y);

		if (!smoothVerticalPosition.IsEqualApprox(verticalPos)) {
			// The camera's new vertical speed is based on the camera's current vertical velocity
			// The camera's vertical movement gets faster as the player keeps moving vertically
			float targetVerticalTime = Mathf.Lerp(VerticalSmoothTime, HorizontalSmoothTime, Mathf.Clamp(verticalVelocity.LengthSquared(), 0f, 1f));

			float transitionSpeed = targetVerticalTime > verticalTime ? 1.5f : 0.5f; // Accelerate faster than decelerate

			verticalTime = Mathf.Lerp(verticalTime, targetVerticalTime, transitionSpeed * floatDelta);
		}
		smoothVerticalPosition = smoothVerticalPosition.SmoothDamp(verticalPos, ref verticalVelocity, verticalTime, Mathf.Inf, floatDelta);


		// Make The Camera Movement slower on the Y axis than on the X axis
		Vector3 horizontalPos = Subject.Origin - verticalPos;
		if (!smoothHorizontalPosition.IsEqualApprox(horizontalPos)) {
			smoothHorizontalPosition = smoothHorizontalPosition.SmoothDamp(horizontalPos, ref horizontalVelocity, HorizontalSmoothTime, Mathf.Inf, floatDelta);
		}

		base._Process(delta);
	}
}