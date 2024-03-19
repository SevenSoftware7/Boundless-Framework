namespace LandlessSkies;

using System;
using Godot;

public sealed partial class JoypadInputDevice : InputDevice {
	public override float Sensitivity => 0.025f;

	public int Id { get; init; }
	public new string Name { get; private set; } = string.Empty;
	public string GUID { get; private set; } = Guid.Empty.ToString();

	protected override StringName ActionSuffix => $"joy{Id}";

	private JoypadInputDevice() : base() { }
	public JoypadInputDevice(int DeviceId) : base() {
		Id = DeviceId;
	}

	protected override InputEvent ConvertEvent(InputEvent @event) {
		InputEvent newEvent = (@event.Duplicate() as InputEvent)!;
		newEvent.Device = Id;
		return newEvent;
	}
	protected override bool IsEventSupported(InputEvent @event) => @event is InputEventJoypadButton || @event is InputEventJoypadMotion;

	public override void Connect() {
		base.Connect();

		Name = Input.GetJoyName(Id);
		GUID = Input.GetJoyGuid(Id);
		GD.Print($"Device {Id} ({Name}) Connected");
	}

	public override void Disconnect() {
		base.Disconnect();

		GD.Print($"Device {Id} ({Name}) Disconnected");
	}

}
