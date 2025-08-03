using Godot;

namespace Seven.Boundless;

[Tool]
[GlobalClass]
public partial class SetModifier : TraitModifier {
	[Export]
	public float Value {
		get => _value;
		set {
			_value = value;
			EmitValueModified();
		}
	}
	private float _value;

	[Export] public bool _isStacking = false;
	public override bool IsStacking => _isStacking;


	public SetModifier(Trait target, float value, bool isStacking = false) : base(target) {
		_value = value;
		_isStacking = isStacking;
	}
	private SetModifier() : base() { }


	public override float ApplyTo(float baseValue, float multiplier = 1f) {
		return Mathf.Lerp(baseValue, _value, multiplier);
	}

	protected override string GetResourceName() {
		return $"{Trait.Name} = {_value}";
	}
}