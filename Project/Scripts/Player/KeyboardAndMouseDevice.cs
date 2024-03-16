namespace LandlessSkies.Core;

using System;
using System.Diagnostics;
using Godot;

[Tool]
[GlobalClass]
public partial class KeyboardAndMouseDevice : ControlDevice {
	private static readonly StringName EvadeName = "KeyboardEvade";
	private static readonly StringName JumpName = "KeyboardJump";
	private static readonly StringName LightAttackName = "KeyboardLightAttack";
	private static readonly StringName HeavyAttackName = "KeyboardHeavyAttack";
	private static readonly StringName MoveForwardName = "KeyboardMoveForward";
	private static readonly StringName MoveBackwardName = "KeyboardMoveBackward";
	private static readonly StringName MoveLeftName = "KeyboardMoveLeft";
	private static readonly StringName MoveRightName = "KeyboardMoveRight";

	private static InputEventMouseMotion mouseMotion = new();



	public override Vector2 GetVector(MotionType motion, float deadzone = -1) {
		if (motion == MotionType.Look) {
			return mouseMotion.Relative * new Vector2(1, -1);
		}
		return base.GetVector(motion, deadzone);
	}


	protected override StringName GetActionName(InputType input) {
		return input switch {
			InputType.Evade                 => EvadeName,
			InputType.Jump                  => JumpName,
			InputType.LightAttack           => LightAttackName,
			InputType.HeavyAttack           => HeavyAttackName,
			_ when Enum.IsDefined(input)    => throw new UnreachableException($"Undefined Action Name for {input}"),
			_                               => throw new ArgumentOutOfRangeException(nameof(input))
		};
	}
	protected override StringName GetActionName(MotionType motion, MotionDirection direction) {
		return motion switch {
			MotionType.Move					=> direction switch {
				MotionDirection.Forward		=> MoveForwardName,
				MotionDirection.Backward	=> MoveBackwardName,
				MotionDirection.Left		=> MoveLeftName,
				MotionDirection.Right		=> MoveRightName,
				_							=> throw new ArgumentOutOfRangeException(nameof(direction))
			},
			MotionType.Look                 => string.Empty,
			_ when Enum.IsDefined(motion)   => throw new UnreachableException($"Undefined Action Name for {motion}"),
			_                               => throw new ArgumentOutOfRangeException(nameof(motion))
		};
	}

	public override void _Ready() {
		base._Ready();

		if (Engine.IsEditorHint())
			return;

		RebindInput(InputType.Evade, new InputEventKey() { PhysicalKeycode = Key.Shift });
		RebindInput(InputType.Jump, new InputEventKey() { PhysicalKeycode = Key.Space });
		RebindInput(InputType.LightAttack, new InputEventMouseButton() { ButtonIndex = MouseButton.Left });
		RebindInput(InputType.HeavyAttack, new InputEventMouseButton() { ButtonIndex = MouseButton.Right });
		RebindInput(MotionType.Move, MotionDirection.Forward, new InputEventKey() { PhysicalKeycode = Key.W });
		RebindInput(MotionType.Move, MotionDirection.Backward, new InputEventKey() { PhysicalKeycode = Key.S });
		RebindInput(MotionType.Move, MotionDirection.Left, new InputEventKey() { PhysicalKeycode = Key.A });
		RebindInput(MotionType.Move, MotionDirection.Right, new InputEventKey() { PhysicalKeycode = Key.D });
	}

	public override void _ExitTree() {
		base._ExitTree();

		if (Engine.IsEditorHint())
			return;
		if (this.IsEditorExitTree())
			return;

		UnbindInput(InputType.Evade);
		UnbindInput(InputType.Jump);
		UnbindInput(InputType.LightAttack);
		UnbindInput(InputType.HeavyAttack);
		UnbindInput(MotionType.Move, MotionDirection.Forward);
		UnbindInput(MotionType.Move, MotionDirection.Backward);
		UnbindInput(MotionType.Move, MotionDirection.Left);
		UnbindInput(MotionType.Move, MotionDirection.Right);
	}

	public override void _Process(double delta) {
		base._Process(delta);

		if (Engine.IsEditorHint())
			return;

		Callable.From(UpdateMouse).CallDeferred();

		static void UpdateMouse() {
			mouseMotion = new();
		}
	}

	public override void _Input(InputEvent @event) {
		base._Input(@event);

		if (Engine.IsEditorHint())
			return;

		if (@event is InputEventMouseMotion mouseMotion) {
			KeyboardAndMouseDevice.mouseMotion = mouseMotion;
		}
	}
}
