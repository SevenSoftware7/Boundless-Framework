namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public sealed partial class PercentileModifier : TraitModifier {
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



	public PercentileModifier(Trait target, float percentile, bool isStacking = false) : base(target) {
		_percentile = percentile;
		_isStacking = isStacking;
	}
	private PercentileModifier() : base() { }



	public override float ApplyTo(float baseValue) {
		return baseValue * (1f + Mathf.Lerp(0, _percentile, Efficiency));
	}

	protected override string GetResourceName() {
		return $"{_percentile:+0.##%;-#.##%} {TraitName}";
	}

	protected override bool EqualsInternal(TraitModifier other) {
		return other is PercentileModifier percentileModifier && percentileModifier._percentile == _percentile;
	}
	public override int GetHashCode() {
		unchecked {
			return (base.GetHashCode() * 397) ^ (_percentile.GetHashCode() * 397);
		}
	}
}