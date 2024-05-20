namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;

[Tool]
[GlobalClass]
public abstract partial class GaugeControl : Control {
	protected readonly TimeDuration DamagedTimer = new();

	[Export] public float DamageDelay {
		get => DamagedTimer.DurationMsec / 1000f;
		private set => DamagedTimer.DurationMsec = (ulong)(value * 1000);
	}

	[Export] public Gauge? Value;
}
