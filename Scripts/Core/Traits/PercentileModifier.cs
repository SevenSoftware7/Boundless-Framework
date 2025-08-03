namespace Seven.Boundless;

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



	public override float ApplyTo(float baseValue, float multiplier = 1f) {
		return baseValue * Mathf.Lerp(0, _percentile / 100f, multiplier);
	}

	protected override string GetResourceName() {
		return $"{_percentile/100f:0.##%;-#.##%} {Trait.Name}";
	}
}