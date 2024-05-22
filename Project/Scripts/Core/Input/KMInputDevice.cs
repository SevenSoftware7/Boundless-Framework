namespace LandlessSkies.Core;

using Godot;

public sealed partial class KMInputDevice : InputDevice {
	private static InputEventMouseMotion mouseMotion = new();

	public override float Sensitivity => 0.00325f;

	protected override StringName ActionSuffix => suffix;
	private static readonly StringName suffix = "km";


	protected override bool IsEventSupported(InputEvent @event) => @event is InputEventMouse || @event is InputEventKey;

	public override float GetActionStrength(StringName action) => action.ToString() switch {
		_ when !DeviceConnected => base.GetActionStrength(action),
		"look_up" => Mathf.Max(-mouseMotion.ScreenRelative.Y, base.GetActionStrength(action)),
		"look_down" => Mathf.Max(mouseMotion.ScreenRelative.Y, base.GetActionStrength(action)),
		"look_left" => Mathf.Max(-mouseMotion.ScreenRelative.X, base.GetActionStrength(action)),
		"look_right" => Mathf.Max(mouseMotion.ScreenRelative.X, base.GetActionStrength(action)),
		_ => base.GetActionStrength(action),
	};
	public override float GetActionRawStrength(StringName action) => action.ToString() switch {
		"look_up" or "look_down" or "look_left" or "look_right" => GetActionStrength(action),
		_ => base.GetActionRawStrength(action),
	};

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
