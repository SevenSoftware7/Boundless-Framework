namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public sealed partial class PercentileModifier : AttributeModifier {
	[Export]
	public float Percentile {
		get => _percentile;
		set {
			_percentile = value;
			EmitValueModified();
		}
	}
	private float _percentile;

	[Export] public bool _isStacking = false;
	public override bool IsStacking => _isStacking;



	public PercentileModifier(EntityAttribute target, float percentile, bool isStacking = false) : base(target) {
		_percentile = percentile;
		_isStacking = isStacking;
	}
	private PercentileModifier() : base() { }



	public override float ApplyTo(float baseValue) {
		return baseValue * (1f + _percentile);
	}

	protected override string GetResourceName() {
		return $"{_percentile:+0.##%;-#.##%} {AttributeName}";
	}
}