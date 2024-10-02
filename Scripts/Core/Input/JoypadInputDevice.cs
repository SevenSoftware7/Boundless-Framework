namespace LandlessSkies.Core;

using Godot;

public sealed partial class JoypadInputDevice(int DeviceId) : InputDevice() {
	public override float Sensitivity => 0.025f;

	protected override StringName DeviceSuffix => _deviceSuffix;
	private readonly StringName _deviceSuffix = $"joy{DeviceId}";

	public int DeviceId { get; init; } = DeviceId;
	public string DeviceName { get; private set; } = string.Empty;
	public string DeviceGUID { get; private set; } = string.Empty;



	protected override InputEvent ConvertEvent(InputEvent @event) {
		InputEvent newEvent = (@event.Duplicate() as InputEvent)!;
		newEvent.Device = DeviceId;
		return newEvent;
	}
	protected override bool IsEventSupported(InputEvent @event) => @event is InputEventJoypadButton || @event is InputEventJoypadMotion;


	public override void Connect() {
		base.Connect();

		DeviceName = Input.GetJoyName(DeviceId);
		DeviceGUID = Input.GetJoyGuid(DeviceId);
		GD.Print($"Device {DeviceId} ({DeviceName}) Connected");
	}

	public override void Disconnect() {
		base.Disconnect();

		GD.Print($"Device {DeviceId} ({DeviceName}) Disconnected");
	}

}
