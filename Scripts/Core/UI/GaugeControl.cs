namespace LandlessSkies.Core;

using Godot;
using SevenDev.Boundless.Utility;

[GlobalClass]
public abstract partial class GaugeControl : Control {
	protected readonly TimeDuration DamagedTimer = new(false);

	[Export]
	public float DamageDelay {
		get => DamagedTimer.DurationMsec / 1000f;
		private set => DamagedTimer.DurationMsec = (ulong)(value * 1000);
	}

	[Export]
	public Gauge? Value {
		get => _value;
		set {
			if (value == _value) return;

			Callable onMaximumChanged = Callable.From<float>(OnMaximumChanged);
			Callable onValueChanged = Callable.From<float>(OnValueChanged);
			NodeExtensions.SwapSignalEmitter(ref _value, value, Gauge.SignalName.MaximumChanged, onMaximumChanged);
			NodeExtensions.SwapSignalEmitter(ref _value, value, Gauge.SignalName.ValueChanged, onValueChanged);

			if (_value is not null) {
				OnMaximumChanged(_value.Value);
				OnValueChanged(_value.Value);
			}
		}
	}
	private Gauge? _value;

	protected virtual void OnMaximumChanged(float value) { }
	protected virtual void OnValueChanged(float value) { }
}
