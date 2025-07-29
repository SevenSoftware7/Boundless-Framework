namespace Seven.Boundless;

using Godot;

[Tool]
[GlobalClass]
public sealed partial class AdditiveModifier : TraitModifier {
	[Export]
	public float Adder {
		get => _adder;
		set {
			_adder = value;
			EmitValueModified();
		}
	}
	private float _adder;



	public AdditiveModifier(Trait target, float adder) : base(target) {
		_adder = adder;
	}
	private AdditiveModifier() : base() { }



	public override float ApplyTo(float baseValue, float multiplier = 1f) {
		return baseValue + _adder * multiplier;
	}

	protected override string GetResourceName() {
		return $"{(_adder >= 0 ? '+' : '-')} {Mathf.Abs(_adder)} {Trait.Name}";
	}
}