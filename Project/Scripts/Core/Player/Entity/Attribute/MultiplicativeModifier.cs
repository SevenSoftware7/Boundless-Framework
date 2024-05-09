namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public sealed partial class MultiplicativeModifier : AttributeModifier {
	[Export] public float Multiplier {
		get => _multiplier;
		private set {
			_multiplier = value;
			UpdateName();
		}
	}
	private float _multiplier;

	public override float Apply(float baseValue) {
		return baseValue * _multiplier;
	}

	protected override string GetResourceName() {
		return $"{_multiplier:0.##%} {Name}";
	}
}