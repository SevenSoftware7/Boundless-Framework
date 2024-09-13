using Godot;

namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class SetModifier : AttributeModifier {
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


	public SetModifier(EntityAttribute target, float value, bool isStacking = false) : base(target) {
		_value = value;
		_isStacking = isStacking;
	}
	private SetModifier() : base() { }


	public override float ApplyTo(float baseValue) {
		return _value;
	}

	protected override string GetResourceName() {
		return $"{AttributeName} = {_value}";
	}
}