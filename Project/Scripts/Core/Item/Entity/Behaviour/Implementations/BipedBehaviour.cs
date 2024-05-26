namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;

[Tool]
[GlobalClass]
public partial class BipedBehaviour : GroundedBehaviour, IPlayerHandler {
	private float _moveSpeed;
	private MovementType _movementType;

	private Vector3 _lastDirection;

	private PromptControl? interactPrompt;
	private PointerControl? interactPointer;

	public bool HasSetupPlayer => interactPrompt is not null && interactPointer is not null;


	protected BipedBehaviour() : base() { }
	public BipedBehaviour(Entity entity) : base(entity) { }


	public override void Start(EntityBehaviour? previousBehaviour) {
		base.Start(previousBehaviour);

		interactPointer?.Enable();

	}
	public override void Stop() {
		base.Stop();

		interactPrompt?.Update(false);
		interactPointer?.Disable();
	}


	public override void SetupPlayer(Player player) {
		base.SetupPlayer(player);

		interactPrompt ??= player.HudManager.AddPrompt(Entity?.HudPack.InteractPrompt);
		interactPointer ??= player.HudManager.AddPointer(Entity?.HudPack.InteractPointer);
	}

	public override void HandlePlayer(Player player) {
		base.HandlePlayer(player);

		float speedSquared = _inputDirection.LengthSquared();
		MovementType speed = speedSquared switch {
			_ when Mathf.IsZeroApprox(speedSquared) => MovementType.Idle,
			_ when speedSquared <= 0.25f || player.InputDevice.IsActionPressed(Inputs.Walk) => MovementType.Walk,
			_ when player.InputDevice.IsActionPressed(Inputs.Evade) => MovementType.Sprint,
			_ => MovementType.Run
		};
		SetMovementType(speed);


		HandleInteraction(player);
	}

	public override void DisavowPlayer() {
		base.DisavowPlayer();

		interactPrompt?.Destroy();
		interactPrompt = null;

		interactPointer?.QueueFree();
		interactPointer = null;
	}

	private void HandleInteraction(Player player) {
		if (Entity is null) return;

		InteractTarget? target = InteractTarget.GetBestTarget(Entity, 3.25f);

		if (target is not null) {
			if (player.InputDevice.IsActionJustPressed("interact") && target.Interactable.IsInteractable(Entity)) {
				target.Interactable.Interact(Entity, player, target.ShapeIndex);
			}
		}


		if (interactPrompt is not null) {
			interactPrompt.Update(target);
			interactPrompt.SetKey(player.InputDevice.GetActionSymbol("interact"));
		}

		if (interactPointer is not null) {
			interactPointer.Target = target?.Interactable.GlobalTransform;
		}
	}

	public override bool SetMovementType(MovementType speed) {
		if (! base.SetMovementType(speed)) return false;
		if (speed == _movementType) return false;

		_movementType = speed;
		return true;
	}



	public override void _Process(double delta) {
		base._Process(delta);

		if (Engine.IsEditorHint()) return;
		if (Entity is null) return;


		float floatDelta = (float)delta;


		// ---- Speed Calculation ----

		float newSpeed = _movementType switch {
			MovementType.Walk => Entity.Stats.SlowSpeed,
			MovementType.Run => Entity.Stats.BaseSpeed,
			MovementType.Sprint => Entity.Stats.SprintSpeed,
			_ => 0f
		};
		newSpeed = Entity.AttributeModifiers.Get(Attributes.GenericMoveSpeed).ApplyTo(newSpeed);

		float speedDelta = _moveSpeed < newSpeed ? Entity.Stats.Acceleration : Entity.Stats.Deceleration;
		_moveSpeed = Mathf.MoveToward(_moveSpeed, newSpeed, speedDelta * floatDelta);


		// ----- Rotation & Movement -----

		if (_movementType != MovementType.Idle) {
			Vector3 normalizedInput = _inputDirection.Normalized();

			_lastDirection = _lastDirection.Lerp(normalizedInput, Entity.Stats.RotationSpeed * floatDelta);
			Entity.GlobalForward = Entity.GlobalForward.SafeSlerp(normalizedInput, Entity.Stats.RotationSpeed * floatDelta);

			Entity.Movement = _lastDirection * _moveSpeed * Mathf.Clamp(_lastDirection.Dot(Entity.GlobalForward), 0f, 1f);
		} else {
			Entity.Movement = _lastDirection * _moveSpeed;
		}


		Basis newRotation = Basis.LookingAt(Entity.GlobalForward, Entity.UpDirection);
		Entity.GlobalBasis = Entity.GlobalBasis.SafeSlerp(newRotation, (float)delta * Entity.Stats.RotationSpeed);

		_movementType = MovementType.Idle;
	}


	public override void _ExitTree() {
		base._ExitTree();
		interactPrompt?.QueueFree();
		interactPointer?.QueueFree();
	}
}