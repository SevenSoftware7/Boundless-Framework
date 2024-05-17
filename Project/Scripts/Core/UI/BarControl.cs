namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;

[Tool]
[GlobalClass]
public partial class BarControl : Control {
	[Export] public float DamageDelay {
		get => _damagedTimer.DurationMsec / 1000f;
		set => _damagedTimer.DurationMsec = (ulong)(value * 1000);
	}
	[Export] public Health? Value {
		get => _value;
		private set {
			// if (_value == value) return;
			if (this.IsInitializationSetterCall()) {
				_value = value;
				return;
			}

			Callable onValueChanged = new(this, MethodName.OnValueChanged);
			NodeExtensions.SwapSignalEmitter(ref _value, value, Health.SignalName.HealthChange, onValueChanged, ConnectFlags.Persist);

			if (Value is null)
				return;

			if (Bar is not null) {
				Bar.Value = Value.Amount;
				Bar.MaxValue = Value.MaxAmount;
			}
			if (DamagedBar is not null) {
				DamagedBar.Value = Value.Amount;
				DamagedBar.MaxValue = Value.MaxAmount;
			}
		}
	}
	private Health? _value = null!;

	private TimeDuration _damagedTimer = new(1000);
	private float _damagedVelocity;

	[Export] public TextureProgressBar Bar { get; private set; } = null!;
	[Export] public TextureProgressBar DamagedBar { get; private set; } = null!;


	public void OnValueChanged(float amount) {
		_damagedTimer.Start();
	}

	public override void _Ready() {
		base._Ready();

		Callable onValueChanged = new(this, MethodName.OnValueChanged);
		_value?.ReconnectSignal(Health.SignalName.HealthChange, onValueChanged, ConnectFlags.Persist);
	}

	public override void _Process(double delta) {
		base._Process(delta);

		if (Value is null || Bar is null)
			return;

		Size = new Vector2(Value.MaxAmount * 8f, GetCombinedMinimumSize().Y);

		Bar.MaxValue = Value.MaxAmount;
		Bar.Value = Mathf.Lerp(Value.Amount, Value.Amount, 2f * (float)delta);

		if (DamagedBar is null)
			return;

		DamagedBar.MaxValue = Value.MaxAmount;

		if (DamagedBar.Value < Value.Amount) {
			DamagedBar.Value = Value.Amount;
			return;
		}

		if (!_damagedTimer) {
			_damagedVelocity = 0f;
			return;
		}

		DamagedBar.Value = MathUtility.SmoothDamp((float)DamagedBar.Value, (float)Bar.Value, ref _damagedVelocity, 0.15f, Mathf.Inf, (float)delta);
	}
}
