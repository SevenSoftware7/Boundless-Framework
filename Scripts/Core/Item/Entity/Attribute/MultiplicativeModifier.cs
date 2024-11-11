namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public sealed partial class MultiplicativeModifier : AttributeModifier {
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



	public MultiplicativeModifier(EntityAttribute target, float multiplier, bool isStacking = false) : base(target) {
		_multiplier = multiplier;
		_isStacking = isStacking;
	}
	private MultiplicativeModifier() : base() { }



	public override float ApplyTo(float baseValue) {
		return baseValue * Mathf.Lerp(1f, _multiplier, Efficiency);
	}

	protected override string GetResourceName() {
		return $"{_multiplier:0.##%} {AttributeName}";
	}

	protected override bool EqualsInternal(AttributeModifier other) {
		return other is MultiplicativeModifier multiplicativeModifier && multiplicativeModifier._multiplier == _multiplier;
	}
	public override int GetHashCode() {
		unchecked {
			return (base.GetHashCode() * 397) ^ (_multiplier.GetHashCode() * 397);
		}
	}
}