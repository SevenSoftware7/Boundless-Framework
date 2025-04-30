namespace LandlessSkies.Core;

using Godot;

public sealed partial class KMInputDevice : InputDevice {
	private static readonly StringName suffix = "km";
	private static InputEventMouseMotion mouseMotion = new();

	public override float Sensitivity => 0.00325f;
	public override StringName FullName => "Keyboard & Mouse";

	protected override StringName DeviceSuffix => suffix;


	protected override bool IsEventSupported(InputEvent @event) => @event is InputEventMouse || @event is InputEventKey;

	public override float GetActionStrength(StringName action)/*  => action.ToString() switch {
		_ when !DeviceConnected => base.GetActionStrength(action),
		"look_up" => Mathf.Max(-mouseMotion.ScreenRelative.Y, 0f),
		"look_down" => Mathf.Max(mouseMotion.ScreenRelative.Y, 0f),
		"look_left" => Mathf.Max(-mouseMotion.ScreenRelative.X, 0f),
		"look_right" => Mathf.Max(mouseMotion.ScreenRelative.X, 0f),
		_ => base.GetActionStrength(action),
	}; */ {
		if (!DeviceConnected) return 0f;
		if (action == Inputs.LookUp) return Mathf.Max(-mouseMotion.ScreenRelative.Y, 0f);
		if (action == Inputs.LookDown) return Mathf.Max(mouseMotion.ScreenRelative.Y, 0f);
		if (action == Inputs.LookLeft) return Mathf.Max(-mouseMotion.ScreenRelative.X, 0f);
		if (action == Inputs.LookRight) return Mathf.Max(mouseMotion.ScreenRelative.X, 0f);
		return base.GetActionStrength(action);
	}

	public override float GetActionRawStrength(StringName action)/*  => action.ToString() switch {
		"look_up" or "look_down" or "look_left" or "look_right" => GetActionStrength(action),
		_ => base.GetActionRawStrength(action),
	}; */ {
		if (!DeviceConnected) return 0f;
		if (action == Inputs.LookUp || action == Inputs.LookDown || action == Inputs.LookLeft || action == Inputs.LookRight)
			return GetActionStrength(action);

		return base.GetActionRawStrength(action);
	}


	public override void _Process(double delta) {
		base._Process(delta);

		if (Engine.IsEditorHint()) return;

		Callable.From(() => {
			mouseMotion = new();
		}).CallDeferred(); // Do at the end of the Frame


	}

	public override void _UnhandledInput(InputEvent @event) {
		base._UnhandledInput(@event);

		if (Engine.IsEditorHint()) return;

		if (@event is InputEventMouseMotion mouseMotion) {
			KMInputDevice.mouseMotion = mouseMotion;
		}
	}

}
