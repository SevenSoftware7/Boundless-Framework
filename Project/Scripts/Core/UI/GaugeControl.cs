namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;

[Tool]
[GlobalClass]
public partial class GaugeControl : Control {
	private TimeDuration _damagedTimer = new(1000);
	private double _damagedVelocity;
	[Export] public float DamageDelay {
		get => _damagedTimer.DurationMsec / 1000f;
		set => _damagedTimer.DurationMsec = (ulong)(value * 1000);
	}

	[Export] public Gauge? Value {
		get => _value;
		set {
			if (_value == value) return;
			if (this.IsInitializationSetterCall()) {
				_value = value;
				return;
			}

			Callable onValueChanged = new(this, MethodName.OnValueChanged);
			NodeExtensions.SwapSignalEmitter(ref _value, value, Gauge.SignalName.HealthChange, onValueChanged, ConnectFlags.Persist);
		}
	}
	private Gauge? _value = null!;


	[Export] public TextureProgressBar? Bar { get; private set; }
	[Export] public TextureProgressBar? DamagedBar { get; private set; }


	public void OnValueChanged(float damageAmount) {
		if (damageAmount < 0) {
			_damagedTimer.Start();
		}
	}

	public override void _Ready() {
		base._Ready();

		Callable onValueChanged = new(this, MethodName.OnValueChanged);
		_value?.ReconnectSignal(Gauge.SignalName.HealthChange, onValueChanged, ConnectFlags.Persist);
	}

	public override void _Process(double delta) {
		base._Process(delta);

		if (Value is null || Bar is null) return;
		Size = new Vector2(Value.MaxAmount * 8f, GetCombinedMinimumSize().Y);

		Bar.MaxValue = Value.MaxAmount;

		Bar.Value = Value.Amount;


		if (DamagedBar is null) return;

		DamagedBar.MaxValue = Value.MaxAmount;

		if (DamagedBar.Value < Value.Amount) {
			DamagedBar.Value = Value.Amount;
		}
		else if (!_damagedTimer) {
			_damagedVelocity = 0f;
		}
		else {
			DamagedBar.Value = DamagedBar.Value.SmoothDamp(Value.Amount, ref _damagedVelocity, 0.15f, Mathf.Inf, delta);
		}
	}
}
