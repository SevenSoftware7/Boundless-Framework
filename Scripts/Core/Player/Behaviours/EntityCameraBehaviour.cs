namespace LandlessSkies.Core;

using Godot;
using SevenDev.Boundless.Utility;

public abstract partial class EntityCameraBehaviour : CameraBehaviour {
	public sealed override Transform3D Transform => transform;
	public abstract Vector3 Velocity { get; protected set; }

	[Export] public Vector3 CameraOffset {
		get => _cameraOffset;
		set {
			_cameraOffset = value;
			_offsetMagnitude = _cameraOffset.Length();
		}
	}
	private Vector3 _cameraOffset;
	private float _offsetMagnitude;

	[Export] public Transform3D Subject { get; protected set; } = Transform3D.Identity;
	[Export] public Basis LocalRotation { get; protected set; } = Basis.Identity;
	private float _smoothDistance = 0f;

	private Transform3D transform = Transform3D.Identity;


	private EntityCameraBehaviour() : this(null) { }
	protected EntityCameraBehaviour(CameraController3D? camera) : base(camera) {
		CameraOffset = new(1f, -0.12f, 5.2f);
	}


	public abstract void MoveCamera(Vector2 cameraInput);

	public void SetEntityAsSubject(Entity entity) {
		Transform3D entityTransform = entity.Transform;
		Vector3 origin = entity.CenterOfMass?.GlobalPosition ?? entityTransform.Origin;

		if (entity.Skeleton is not null && entity.Skeleton.TryGetBonePosition("Head", out var bonePosition)) {
			origin = bonePosition;
		}

		Subject = entityTransform with {
			Basis = Subject.Basis * BasisExtensions.FromToBasis(Subject.Basis.Y, entity.UpDirection),
			Origin = origin
		};
	}


	protected override void _Start(CameraBehaviour? previousBehaviour = null) {
		base._Start(previousBehaviour);

		Input.MouseMode = Input.MouseModeEnum.Captured;

		if (previousBehaviour is EntityCameraBehaviour other) {
			Subject = other.Subject;
			CameraOffset = other.CameraOffset;
			LocalRotation = other.LocalRotation;
			TargetPosition = other.TargetPosition;
			Velocity = other.Velocity;
		}

		transform = CameraController.GlobalTransform;
	}

	public override void _Process(double delta) {
		base._Process(delta);
		if (!IsActive) return;

		float floatDelta = (float)delta;


		Basis targetBasis = Subject.Basis * LocalRotation;

		Vector3 offsetDirection = Mathf.IsZeroApprox(_offsetMagnitude)
			? Vector3.Zero
			: (_cameraOffset / _offsetMagnitude);

		Vector3 absoluteOffset = targetBasis * offsetDirection;


		ComputeWallCollision(TargetPosition, absoluteOffset, _offsetMagnitude, ref _smoothDistance, floatDelta);

		Vector3 finalPos = TargetPosition + absoluteOffset * _smoothDistance;
		transform = new(targetBasis, finalPos);
	}
}