using Godot;
using Godot.Collections;
using SevenGame.Utility;

namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class Health() : Node() {
	
	private TimeDuration _damagedHealthTimer = new(1000);
	private float _damagedHealthVelocity = 0f;



	[Export] public float MaxAmount {
		get => _maxAmount;
		set {
			_maxAmount = Mathf.Max(value, 1f);
			Amount = Mathf.Min(Amount, _maxAmount);

			NotifyPropertyListChanged();
		}
	}
	private float _maxAmount = 100f;

	[Export] public float Amount {
		get => _amount;
		set {
			_amount = Mathf.Clamp(value, 0f, _maxAmount);

			_damagedHealthTimer.Start();
			EmitSignal(SignalName.HealthChange, Amount);
		}
	}
	private float _amount;

	[Export] public float DamagedHealth { get; private set; }


	[Signal] public delegate void HealthChangeEventHandler(float amount);



	public Health(float max) : this() {
		_maxAmount = max;
	}



	public override void _Ready() {
		base._Ready();

		_amount = _maxAmount;
		DamagedHealth = _amount;
	}

	public override void _Process(double delta) {
		base._Process(delta);

		if ( DamagedHealth < Amount ) {
			DamagedHealth = Amount;
			return;
		}

		if ( ! _damagedHealthTimer.IsDone ) {
			_damagedHealthVelocity = 0f;
			return;
		}

		DamagedHealth = MathUtility.SmoothDamp(DamagedHealth, _amount, ref _damagedHealthVelocity, 0.15f, Mathf.Inf, (float)delta);
	}

	public override void _ValidateProperty(Dictionary property) {
		base._ValidateProperty(property);
		
		StringName name = property["name"].AsStringName();
		
		if (name == PropertyName.DamagedHealth) {
			property["usage"] = (int)(property["usage"].As<PropertyUsageFlags>() & ~PropertyUsageFlags.Storage | PropertyUsageFlags.ReadOnly);
		}
		
		if (
			name == PropertyName.Amount ||
			name == PropertyName.DamagedHealth
		) {
			property["hint"] = (int)PropertyHint.Range;
			property["hint_string"] = $"0,{MaxAmount},";
		}
	}
}