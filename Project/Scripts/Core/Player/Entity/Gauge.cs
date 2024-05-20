namespace LandlessSkies.Core;

using Godot;
using Godot.Collections;
using SevenDev.Utility;


[Tool]
[GlobalClass]
public partial class Gauge : Node {
	[Export] public float MaxAmount {
		get => _maxAmount;
		private set => SetMaximum(value, StaticRatio);
	}
	private float _maxAmount = 100f;

	[Export] public float Amount {
		get => _amount;
		set {
			float oldAmount = _amount;
			_amount = Mathf.Clamp(value, 0f, _maxAmount);

			if (_amount == 0f) {
				EmitSignal(SignalName.Emptied, oldAmount);
			}

			EmitSignal(SignalName.HealthChange, _amount - oldAmount);
		}
	}
	private float _amount;

	[Export(PropertyHint.Range, "0,1,")] public float Ratio {
		get => Amount / MaxAmount;
		set => Amount = MaxAmount * value;
	}
	[Export] public bool StaticRatio = false;


	[Signal] public delegate void HealthChangeEventHandler(float amount);
	[Signal] public delegate void EmptiedEventHandler(float fromAmount);



	protected Gauge() : base() {
		_amount = _maxAmount;
	}
	public Gauge(float max) : this() {
		_maxAmount = max;
	}

	public void SetMaximum(float max, bool keepAmountRatio = false) {
		if (max == _maxAmount) return;

		float oldMaxAmount = _maxAmount;
		_maxAmount = Mathf.Max(max, 0f);
		if (this.IsInitializationSetterCall()) return;

		Amount = keepAmountRatio
			? Mathf.Clamp(_amount / oldMaxAmount, 0f, 1f) * _maxAmount
			: Mathf.Min(Amount, _maxAmount);

		NotifyPropertyListChanged();
	}

	public void Kill() {
		Amount = 0f;
	}

	public override void _Ready() {
		base._Ready();
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