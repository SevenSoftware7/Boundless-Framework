namespace LandlessSkies.Core;

using Godot;
using Godot.Collections;

[Tool]
[GlobalClass]
public partial class Gauge : Node {
	[Export]
	public float Maximum {
		get => _maximum;
		private set => SetMaximum(value, StaticRatio);
	}
	private float _maximum = 100f;

	[Export]
	public float Value {
		get => _value;
		set {
			float oldValue = _value;
			_value = Mathf.Clamp(value, 0f, _maximum);

			if (_value == oldValue) return;

			if (_value == 0f) {
				OnEmptied(oldValue);
			}

			OnValueChanged(_value);
		}
	}
	private float _value;

	[Export(PropertyHint.Range, "0,1,")]
	public float Ratio {
		get => Value / Maximum;
		set => Value = Maximum * value;
	}
	[Export] public bool StaticRatio = false;


	[Signal] public delegate void MaximumChangedEventHandler(float amount);
	[Signal] public delegate void ValueChangedEventHandler(float amount);
	[Signal] public delegate void EmptiedEventHandler(float fromAmount);



	protected Gauge() : base() {
		_value = _maximum;
	}
	public Gauge(float maximum) : this() {
		_maximum = maximum;
	}

	public void SetMaximum(float maximum, bool keepRatio = false) {
		float oldMaximum = _maximum;
		_maximum = Mathf.Max(maximum, 0f);

		if (_maximum == oldMaximum) return;

		Value = keepRatio
			? Mathf.Clamp(_value / oldMaximum, 0f, 1f) * _maximum
			: Mathf.Min(Value, _maximum);

		OnMaximumChanged(_maximum);
	}

	public void Kill() {
		Value = 0f;
	}

	public override void _Ready() {
		base._Ready();
	}

	public override void _ValidateProperty(Dictionary property) {
		base._ValidateProperty(property);

		StringName name = property["name"].AsStringName();

		if (name == PropertyName.Value) {
			property["hint"] = (int)PropertyHint.Range;
			property["hint_string"] = $"0,{Maximum},";
		}
	}
}