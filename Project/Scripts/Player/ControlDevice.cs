using Godot;
using SevenGame.Utility;
using System;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public abstract partial class ControlDevice : Node, IDisposable {
	private bool _disposed = false;



	protected abstract StringName GetActionName(InputType input);
	protected abstract StringName GetActionName(MotionType motion, MotionDirection direction);


	public virtual bool IsInputPressed(InputType input) =>
		Input.IsActionPressed(GetActionName(input));

	public virtual bool IsInputJustPressed(InputType input) =>
		Input.IsActionJustPressed(GetActionName(input));

	public virtual bool IsInputJustReleased(InputType input) =>
		Input.IsActionJustReleased(GetActionName(input));

	public virtual Vector2 GetVector(MotionType motion, float deadzone = -1) =>
		Input.GetVector(
			GetActionName(motion, MotionDirection.Left), GetActionName(motion, MotionDirection.Right),
			GetActionName(motion, MotionDirection.Down), GetActionName(motion, MotionDirection.Up),
			deadzone
		);


	protected void RebindInput(InputType input, params InputEvent[] events) {
		StringName actionName = GetActionName(input);
		RebindInput(actionName, events);
	}
	protected void RebindInput(MotionType motion, MotionDirection direction, params InputEvent[] events) {
		StringName actionName = GetActionName(motion, direction);
		RebindInput(actionName, events);
	}
	protected virtual void RebindInput(StringName actionName, params InputEvent[] events) {
		if (actionName.IsEmpty) return;
		
		if ( ! InputMap.HasAction(actionName) ) {
			InputMap.AddAction(actionName);
		}
		InputMap.ActionEraseEvents(actionName);

		for (int i = 0; i < events.Length; i++) {
			InputMap.ActionAddEvent(actionName, events[i]);
		}
	}
	
	protected void UnbindInput(InputType input) {
		StringName actionName = GetActionName(input);
		UnbindInput(actionName);
	}
	protected void UnbindInput(MotionType motion, MotionDirection direction) {
		StringName actionName = GetActionName(motion, direction);
		UnbindInput(actionName);
	}
	protected virtual void UnbindInput(StringName actionName) {
		if (actionName.IsEmpty) return;
		
		if ( ! InputMap.HasAction(actionName) ) {
			InputMap.EraseAction(actionName);
		}
	}



	public enum MotionType {
		Look,
		Move
	}

	public enum MotionDirection : byte {
		Up = 1,
		Down = 2,
		Left = 3,
		Right = 4,

		 // Godot's inverted Z situation makes this confusing
		Forward = Down,
		Backward = Up
	}

	public enum InputType {
		Jump,
		Evade,
		LightAttack,
		HeavyAttack
	}
}