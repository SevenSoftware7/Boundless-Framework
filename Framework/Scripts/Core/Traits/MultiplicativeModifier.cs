namespace Seven.Boundless;

using Godot;

[Tool]
[GlobalClass]
public sealed partial class MultiplicativeModifier : TraitModifier {
	[Export]
	public float Multiplier {
		get => _multiplier;
		set {
			_multiplier = value;
			EmitValueModified();
		}
	}
	private float _multiplier;

	[Export] public bool _isStacking = false;
	public override bool IsStacking => _isStacking;



	public MultiplicativeModifier(Trait target, float multiplier, bool isStacking = false) : base(target) {
		_multiplier = multiplier;
		_isStacking = isStacking;
	}
	private MultiplicativeModifier() : base() { }



	public override float ApplyTo(float baseValue, float multiplier = 1f) {
		return baseValue * Mathf.Lerp(1f, _multiplier, multiplier);
	}

	protected override string GetResourceName() {
		return $"{_multiplier:0.##%} {Trait.Name}";
	}
}