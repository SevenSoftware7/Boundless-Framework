namespace LandlessSkies.Core;

using Godot;
using SevenDev.Boundless.Utility;

public sealed partial class GimbleCameraBehaviour : EntityCameraBehaviour {

	public override Vector3 Velocity {
		get => _velocity;
		protected set => _velocity = value;
	}
	private Vector3 _velocity = Vector3.Zero;

	protected override bool IsOneTime { get; } = false;


	[Export] public float SmoothTime = 0.065f;


	private GimbleCameraBehaviour() : this(null!) { }
	public GimbleCameraBehaviour(CameraController3D controller) : base(controller) { }


	public override void MoveCamera(Vector2 cameraInput) {
		LookRotation *=
			new Basis(LookRotation.Inverse().Y, -cameraInput.X) *
			new Basis(Vector3.Right, cameraInput.Y);
	}

	public override void _Process(double delta) {
		if (!IsActive) return;
		if (Subject is null) return;
		Transform3D subjectTransform = SubjectTransform;

		Transform3D transform = subjectTransform;

		FollowPosition = CameraController.GlobalPosition.SmoothDamp(transform.Origin, ref _velocity, SmoothTime, Mathf.Inf, (float)delta);

		base._Process(delta);
	}
}