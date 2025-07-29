namespace Seven.Boundless;

using Godot;

public sealed partial class JoypadInputDevice(int DeviceId) : InputDevice() {
	public override float Sensitivity => 0.03f;
	public override StringName FullName => $"Joypad {DeviceId} ({DeviceName})";

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


	protected override void OnConnect() {
		DeviceName = Input.GetJoyName(DeviceId);
		DeviceGUID = Input.GetJoyGuid(DeviceId);
	}

}
