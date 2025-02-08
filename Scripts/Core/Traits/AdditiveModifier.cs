namespace LandlessSkies.Core;

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



	public override float ApplyTo(float baseValue) {
		return baseValue + _adder * Efficiency;
	}

	protected override string GetResourceName() {
		return $"{(_adder >= 0 ? '+' : '-')} {Mathf.Abs(_adder)} {Trait.Name}";
	}

	protected override bool EqualsInternal(TraitModifier other) {
		return other is AdditiveModifier additiveModifier && additiveModifier._adder == _adder;
	}
	public override int GetHashCode() {
		unchecked {
			return (base.GetHashCode() * 397) ^ (_adder.GetHashCode() * 397);
		}
	}

}