namespace Seven.Boundless;

using System;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using Godot;
using Seven.Boundless.Utility;


[GlobalClass]
public partial class CameraController3D : Camera3D {
	[Export] public CameraBehaviour? CurrentBehaviour { get; private set; }

	public ServiceContainer Behaviours = new();


	public bool SetOrAddBehaviour<T>(Func<T> creator, [NotNullWhen(true)] out T behaviour) where T : CameraBehaviour {
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

	public bool GetGroundedMovement(Vector3 upDirection, Vector2 moveInput, out Basis camRotation, out Vector3 groundedMovement) {
		Vector3 rawInput = new(moveInput.X, 0, moveInput.Y);
		if (rawInput.LengthSquared().IsZeroApprox()) {
			camRotation = Basis.Identity;
			groundedMovement = Vector3.Zero;
			return false;
		}

		Vector3 camRight = GlobalBasis.X;
		float localAlignment = Mathf.Ceil(upDirection.Dot(GlobalBasis.Y));
		Vector3 targetUp = upDirection * (localAlignment * 2f - 1f);
		Vector3 groundedCamForward = targetUp.Cross(camRight).Normalized();

		camRotation = Basis.LookingAt(groundedCamForward, targetUp);

		groundedMovement = camRotation * rawInput.ClampMagnitude(1f);

		return true;
	}

	public bool GetCameraRelativeMovement(Vector2 moveInput, out Basis camRotation, out Vector3 cameraRelativeMovement) {
		Vector3 rawInput = new(moveInput.X, 0, moveInput.Y);
		if (rawInput.LengthSquared().IsZeroApprox()) {
			camRotation = Basis.Identity;
			cameraRelativeMovement = Vector3.Zero;
			return false;
		}

		camRotation = GlobalBasis;
		cameraRelativeMovement = camRotation * rawInput.ClampMagnitude(1f);
		return true;
	}
}