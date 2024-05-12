using Godot;
using Godot.Collections;

namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class Health : Node {
	[Export] public float MaxAmount {
		get => _maxAmount;
		set {
			_maxAmount = Mathf.Max(value, 0f);
			if (this.IsInitializationSetterCall()) return;

			Amount = Mathf.Min(Amount, _maxAmount);

			NotifyPropertyListChanged();
		}
	}
	private float _maxAmount = 100f;

	[Export] public float Amount {
		get => _amount;
		set {
			float oldAmount = _amount;
			_amount = Mathf.Clamp(value, 0f, _maxAmount);

			if (_amount == 0f) {
				EmitSignal(SignalName.Death, oldAmount);
			}

			EmitSignal(SignalName.HealthChange, Amount);
		}
	}
	private float _amount;


	[Signal] public delegate void HealthChangeEventHandler(float amount);
	[Signal] public delegate void DeathEventHandler(float fromHealth);



	protected Health() : base() { }
	public Health(float max) : this() {
		_maxAmount = max;
	}

	public void Kill() {
		Amount = 0f;
	}

	public override void _Ready() {
		base._Ready();

		_amount = _maxAmount;
	}

	public override void _ValidateProperty(Dictionary property) {
		base._ValidateProperty(property);

		StringName name = property["name"].AsStringName();

		if (name == PropertyName.Amount) {
			property["hint"] = (int)PropertyHint.Range;
			property["hint_string"] = $"0,{MaxAmount},";
		}
	}
}