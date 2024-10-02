namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public sealed partial class AdditiveModifier : AttributeModifier {
	[Export]
	public float Adder {
		get => _adder;
		set {
			_adder = value;
			EmitValueModified();
		}
	}
	private float _adder;



	public AdditiveModifier(EntityAttribute target, float adder) : base(target) {
		_adder = adder;
	}
	private AdditiveModifier() : base() { }



	public override float ApplyTo(float baseValue) {
		return baseValue + _adder;
	}

	protected override string GetResourceName() {
		return $"{(_adder >= 0 ? '+' : '-')} {Mathf.Abs(_adder)} {AttributeName}";
	}
}