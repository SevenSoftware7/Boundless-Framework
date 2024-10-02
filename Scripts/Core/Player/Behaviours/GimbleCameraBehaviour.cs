namespace LandlessSkies.Core;

using Godot;
using SevenDev.Boundless.Utility;

public sealed partial class GimbleCameraBehaviour : EntityCameraBehaviour {
	public override Vector3 TargetPosition {
		get => smoothPosition;
		protected set => smoothPosition = value;
	}
	private Vector3 smoothPosition = Vector3.Zero;

	public override Vector3 Velocity {
		get => velocity;
		protected set => velocity = value;
	}
	private Vector3 velocity = Vector3.Zero;

	protected override bool IsOneTime => false;


	[Export] public float SmoothTime = 0.065f;


	private GimbleCameraBehaviour() : this(null) { }
	public GimbleCameraBehaviour(CameraController3D? controller) : base(controller) { }


	public override void MoveCamera(Vector2 cameraInput) {
		LocalRotation *=
			new Basis(LocalRotation.Inverse().Y, -cameraInput.X) *
			new Basis(Vector3.Right, cameraInput.Y);
	}

	public override void _Process(double delta) {
		Vector3 position = Subject.Origin;

		smoothPosition = smoothPosition.SmoothDamp(position, ref velocity, SmoothTime, Mathf.Inf, (float)delta);

		base._Process(delta);
	}
}