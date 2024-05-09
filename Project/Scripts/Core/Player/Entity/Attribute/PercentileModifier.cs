namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public sealed partial class PercentileModifier : AttributeModifier {
	[Export] public float Percentile {
		get => _percentile;
		private set {
			_percentile = value;
			UpdateName();
		}
	}
	private float _percentile;

	public override float Apply(float baseValue) {
		return baseValue * (1f + _percentile);
	}

	protected override string GetResourceName() {
		return $"{_percentile:+0.##%;-#.##%} {Name}";
	}
}