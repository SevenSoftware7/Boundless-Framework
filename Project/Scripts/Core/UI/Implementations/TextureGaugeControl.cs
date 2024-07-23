namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;

[Tool]
[GlobalClass]
public partial class TextureGaugeControl : GaugeControl {
	[Export] public TextureProgressBar? Bar { get; private set; }
	[Export] public TextureProgressBar? DamagedBar { get; private set; }

	protected double _damagedVelocity;

	protected float _maximumProgress;
	protected float _progress;

	protected override void OnMaximumChanged(float maximum) {
		base.OnMaximumChanged(maximum);
		_maximumProgress = maximum;
	}
	protected override void OnValueChanged(float value) {
		base.OnValueChanged(value);
		_progress = value;
		DamagedTimer.Start();
	}


	public override void _Process(double delta) {
		base._Process(delta);
		if (Bar is null) return;

		Size = new Vector2(_maximumProgress * 8f, GetCombinedMinimumSize().Y);

		Bar.MaxValue = _maximumProgress;

		if (Bar.Value <= _progress) {
			Bar.Value = _progress;
		}
		else if (Bar.Value > _progress) {
			Bar.Value = Mathf.Lerp(Bar.Value, _progress, 25 * delta);
		}


		if (DamagedBar is null) return;


		DamagedBar.MaxValue = _maximumProgress;

		if (DamagedBar.Value < _progress) {
			DamagedBar.Value = _progress;
		}
		else if (!DamagedTimer) {
			_damagedVelocity = 0f;
		}
		else {
			DamagedBar.Value = DamagedBar.Value.SmoothDamp(_progress, ref _damagedVelocity, 0.15f, Mathf.Inf, delta);
		}
	}
}