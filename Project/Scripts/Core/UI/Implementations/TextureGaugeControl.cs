namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;

[Tool]
[GlobalClass]
public partial class TextureGaugeControl : GaugeControl {
	[Export] public TextureProgressBar? Bar { get; private set; }
	[Export] public TextureProgressBar? DamagedBar { get; private set; }

	protected double _damagedVelocity;


	public override void _Process(double delta) {
		base._Process(delta);

		if (Value is null || Bar is null) return;
		Size = new Vector2(Value.MaxAmount * 8f, GetCombinedMinimumSize().Y);

		Bar.MaxValue = Value.MaxAmount;

		if (Bar.Value > Value.Amount) {
			DamagedTimer.Start();
		}
		Bar.Value = Value.Amount;


		if (DamagedBar is null) return;


		DamagedBar.MaxValue = Value.MaxAmount;

		if (DamagedBar.Value < Value.Amount) {
			DamagedBar.Value = Value.Amount;
		}
		else if (! DamagedTimer) {
			_damagedVelocity = 0f;
		}
		else {
			DamagedBar.Value = DamagedBar.Value.SmoothDamp(Value.Amount, ref _damagedVelocity, 0.15f, Mathf.Inf, delta);
		}
	}
}