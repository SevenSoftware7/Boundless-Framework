namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public sealed partial class AdditiveModifier : AttributeModifier {
	[Export] public float Adder {
		get => _adder;
		private set {
			_adder = value;
			UpdateName();
		}
	}
	private float _adder;



	public AdditiveModifier(StringName name, float adder) : base(name) {
		_adder = adder;
	}
	private AdditiveModifier() : base() {}



	public override float ApplyTo(float baseValue) {
		return baseValue + _adder;
	}

	protected override string GetResourceName() {
		return $"+{_adder} {Name}";
	}
}