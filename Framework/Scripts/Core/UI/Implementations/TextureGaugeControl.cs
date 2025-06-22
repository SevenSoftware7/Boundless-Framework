namespace SevenDev.Boundless;

using Godot;
using SevenDev.Boundless.Utility;

[Tool]
[GlobalClass]
public partial class TextureGaugeControl : GaugeControl {
	private const float HP_TO_PXL = 8f;

	[Export] public TextureProgressBar? Bar { get; private set; }
	[Export] public TextureProgressBar? DamagedBar { get; private set; }

	protected double _maximumVelocity;
	protected double _damagedVelocity;

	protected float _maximum;
	protected float _value;

	protected override void OnMaximumChanged(float maximum) {
		base.OnMaximumChanged(maximum);
		_maximum = maximum;
	}
	protected override void OnValueChanged(float value) {
		base.OnValueChanged(value);
		_value = value;
		DamagedTimer.Start();
	}

	public override void _EnterTree() {
		base._EnterTree();

		Size = new Vector2(_maximum * HP_TO_PXL, GetCombinedMinimumSize().Y);

		if (Bar is not null) {
			Bar.MaxValue = _maximum;
			Bar.Value = _value;
		}

		if (DamagedBar is not null) {
			DamagedBar.MaxValue = _maximum;
			DamagedBar.Value = _value;
		}
	}

	public override void _Process(double delta) {
		base._Process(delta);

		if (Bar is null) return;

		Bar.MaxValue = Bar.MaxValue.SmoothDamp(_maximum, ref _maximumVelocity, 0.05f, Mathf.Inf, delta);
		Size = new Vector2((float)Bar.MaxValue * HP_TO_PXL, GetCombinedMinimumSize().Y);


		if (Bar.Value <= _value) {
			Bar.Value = _value;
		}
		else if (Bar.Value > _value) {
			Bar.Value = Mathf.Lerp(Bar.Value, _value, 25 * delta);
		}


		if (DamagedBar is null) return;

		DamagedBar.MaxValue = Bar.MaxValue;

		if (DamagedBar.Value < _value) {
			DamagedBar.Value = _value;
		}
		else if (!DamagedTimer) {
			_damagedVelocity = 0f;
		}
		else {
			DamagedBar.Value = DamagedBar.Value.SmoothDamp(_value, ref _damagedVelocity, 0.15f, Mathf.Inf, delta);
		}
	}
}