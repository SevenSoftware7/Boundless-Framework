namespace LandlessSkies.Core;

using System;
using System.ComponentModel.Design;
using Godot;
using SevenDev.Utility;


// [Tool]
[GlobalClass]
public partial class CameraController3D : Camera3D {
	[Export] public CameraBehaviour? CurrentBehaviour { get; private set; }

	public ServiceContainer Behaviours = new();


	public bool SetOrAddBehaviour<T>(Func<T> creator, out T behaviour) where T : CameraBehaviour {
		Type tType = typeof(T);
		if (CurrentBehaviour is T tBehaviour) {
			behaviour = tBehaviour;
			return true;
		}

		if (Behaviours.GetService(tType) is T existingBehaviour) {
			behaviour = existingBehaviour;
			SetBehaviour(behaviour);

			return true;
		}

		if (creator is null) {
			behaviour = default!;
			return false;
		}

		behaviour = creator.Invoke();
		SetBehaviour(behaviour);

		return true;
	}

	public void SetBehaviour<T>(T behaviour) where T : CameraBehaviour {
		CurrentBehaviour?.Stop(behaviour);

		CameraBehaviour? oldBehaviour = CurrentBehaviour;
		CurrentBehaviour = null;

		if (behaviour is null) return;

		Type tType = typeof(T);
		if (Behaviours.GetService(tType) is null) Behaviours.AddService(tType, behaviour);

		behaviour.Start(oldBehaviour);

		CurrentBehaviour = behaviour.SafeReparentTo(this);
	}

	public void GetGroundedMovement(Vector3 upDirection, Vector2 moveInput, out Basis camRotation, out Vector3 groundedMovement) {
		if (!IsNodeReady()) {
			camRotation = Basis.Identity;
			groundedMovement = Vector3.Zero;
			return;
		}
		Vector3 camRight = GlobalBasis.X;
		float localAlignment = Mathf.Ceil(upDirection.Dot(GlobalBasis.Y));
		Vector3 targetUp = upDirection * (localAlignment * 2f - 1f);
		Vector3 groundedCamForward = targetUp.Cross(camRight).Normalized();

		camRotation = Basis.LookingAt(groundedCamForward, targetUp);

		groundedMovement = camRotation * new Vector3(moveInput.X, 0, moveInput.Y).ClampMagnitude(1f);
	}

	public void GetCameraRelativeMovement(Vector2 moveInput, out Basis camRotation, out Vector3 cameraRelativeMovement) {
		camRotation = GlobalBasis;

		cameraRelativeMovement = camRotation * new Vector3(moveInput.X, 0, moveInput.Y);
	}


	public override void _Process(double delta) {
		base._Process(delta);
		if (CurrentBehaviour is null) return;

		Transform3D transform = CurrentBehaviour.Transform;
		GlobalTransform = transform;
	}
}