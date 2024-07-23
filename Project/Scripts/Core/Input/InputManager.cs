namespace LandlessSkies.Core;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Godot;

// [Tool]
[GlobalClass]
public partial class InputManager : Node {

	private static readonly List<JoypadInputDevice> _joypadDevices = [];
	public static ReadOnlyCollection<JoypadInputDevice> JoypadDevices => _joypadDevices.AsReadOnly();

	public static readonly StringName[] BaseActions = [.. InputMap.GetActions()];



	public static InputDevice CurrentDevice { get; private set; } = null!;
	public static KMInputDevice KeyboardMouseDevice { get; private set; } = null!;


	public static Texture2D ActionSymbol { get; private set; } = null!;

	[Export]
	private Texture2D _actionSymbol {
		get => ActionSymbol;
		set => ActionSymbol = value;
	}


	public InputManager() : base() { }


	private void OnDeviceConnectionChanged(long device, bool connected) {
		int intDevice = (int)device;
		if (connected) {
			OnDeviceConnected(intDevice);
		}
		else {
			OnDeviceDisconnected(intDevice);
		}
	}

	private void OnDeviceConnected(int deviceId) {
		for (; _joypadDevices.Count <= deviceId;) {
			JoypadInputDevice device = new(_joypadDevices.Count);
			_joypadDevices.Add(device);
			AddChild(device);
		}
		_joypadDevices[deviceId].Connect();
	}
	private void OnDeviceDisconnected(int deviceId) {
		if (_joypadDevices.Count > deviceId) {
			_joypadDevices[deviceId].Disconnect();
		}
	}


	public override void _Input(InputEvent @event) {
		base._Input(@event);

		if (@event is InputEventKey || @event is InputEventMouse) {
			CurrentDevice = KeyboardMouseDevice;
			return;
		}

		if (@event is InputEventJoypadButton joypadButton && JoypadDevices.Count > joypadButton.Device) {
			CurrentDevice = JoypadDevices[joypadButton.Device];
			return;
		}
		if (@event is InputEventJoypadMotion joypadMotion && JoypadDevices.Count > joypadMotion.Device) {
			CurrentDevice = JoypadDevices[joypadMotion.Device];
			return;
		}
	}


	public override void _EnterTree() {
		base._EnterTree();

		if (KeyboardMouseDevice is null) {
			KeyboardMouseDevice = new();
			AddChild(KeyboardMouseDevice);
			KeyboardMouseDevice.Connect();
			CurrentDevice = KeyboardMouseDevice;
		}

		Input.JoyConnectionChanged += OnDeviceConnectionChanged;
	}
	public override void _ExitTree() {
		base._ExitTree();
		Input.JoyConnectionChanged -= OnDeviceConnectionChanged;
	}
}