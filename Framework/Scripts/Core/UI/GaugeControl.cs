namespace Seven.Boundless;

using Godot;
using Seven.Boundless.Utility;

[GlobalClass]
public abstract partial class GaugeControl : Control {
	protected readonly Countdown DamagedTimer = new(1000, false);

	[Export]
	public float DamageDelay {
		get => DamagedTimer.DurationMsec / 1000f;
		private set => DamagedTimer.DurationMsec = (ulong)(value * 1000);
	}

	[Export]
	public Gauge? Value {
		get;
		set {
			if (value == field) return;

			Callable onMaximumChanged = Callable.From<float>(OnMaximumChanged);
			Callable onValueChanged = Callable.From<float>(OnValueChanged);
			NodeExtensions.SwapSignalEmitter(ref field, value, Gauge.SignalName.MaximumChanged, onMaximumChanged);
			NodeExtensions.SwapSignalEmitter(ref field, value, Gauge.SignalName.ValueChanged, onValueChanged);

			if (field is not null) {
				OnMaximumChanged(field.Value);
				OnValueChanged(field.Value);
			}
		}
	}

	protected virtual void OnMaximumChanged(float value) { }
	protected virtual void OnValueChanged(float value) { }
}
