namespace LandlessSkies.Core;

using Godot;
using SevenDev.Boundless.Utility;

public abstract partial class EntityCameraBehaviour : CameraBehaviour {
	public abstract Vector3 Velocity { get; protected set; }

	[Export] public Vector3 CameraOffset {
		get => _offsetDirection;
		set {
			_offsetMagnitude = value.Length();
			_offsetDirection = _offsetMagnitude == 0f ? Vector3.Zero : value / _offsetMagnitude;
		}
	}
	private Vector3 _offsetDirection;
	private float _offsetMagnitude;

	[Export] public Entity? Subject;
	public Transform3D? SubjectTransform {
		get {
			if (Subject is null) return null;

			Transform3D entityTransform = Subject.GlobalTransform;
			Vector3 origin = Subject.CenterOfMass?.GlobalPosition ?? entityTransform.Origin;

			if (Subject.Skeleton is not null && Subject.Skeleton.TryGetBonePosition("Head", out var bonePosition)) {
				origin = bonePosition;
			}

			return entityTransform with {
				Basis = Subject.Basis * BasisExtensions.FromToBasis(Subject.Basis.Y, Subject.UpDirection),
				Origin = origin
			};
		}
	}

	[Export] public Vector3 FollowPosition { get; protected set; } = Vector3.Zero;
	[Export] public Basis LocalRotation { get; protected set; } = Basis.Identity;
	[Export] public Basis LookRotation { get; protected set; } = Basis.Identity;
	private float _smoothDistance = 0f;


	protected EntityCameraBehaviour(CameraController3D camera) : base(camera) {
		CameraOffset = new(1f, -0.12f, 5.2f);
	}


	public abstract void MoveCamera(Vector2 cameraInput);


	protected override void _Start(CameraBehaviour? previousBehaviour = null) {
		base._Start(previousBehaviour);

		Input.MouseMode = Input.MouseModeEnum.Captured;

		if (previousBehaviour is EntityCameraBehaviour other) {
			Subject = other.Subject;
			FollowPosition = other.FollowPosition;
			CameraOffset = other.CameraOffset;
			LookRotation = other.LookRotation;
			Velocity = other.Velocity;
		}
	}

	public override void _Process(double delta) {
		base._Process(delta);
		if (!IsActive) return;

		float floatDelta = (float)delta;

		Basis targetBasis = LocalRotation * LookRotation;
		Vector3 targetPosition = FollowPosition;

		Vector3 absoluteOffset = targetBasis * _offsetDirection;


		ComputeWallCollision(targetPosition, absoluteOffset, _offsetMagnitude, ref _smoothDistance, floatDelta);
		Vector3 finalPos = targetPosition + absoluteOffset * _smoothDistance;


		CameraController.GlobalTransform = new(targetBasis, finalPos);
	}
}