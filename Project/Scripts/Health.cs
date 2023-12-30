using Godot;
using SevenGame.Utility;

namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class Health : Node {
	private float _maxAmount = 100f;
	private float _amount;
	
	private float _damagedHealth;
	private TimeInterval _damagedHealthTimer = new();
	private float _damagedHealthVelocity = 0f;



	[Export] public float MaxAmount {
		get => _maxAmount;
		set {
			_maxAmount = Mathf.Max(value, 1f);
			_damagedHealth = Mathf.Min(_damagedHealth, _maxAmount);
		}
	}

	[Export] public float Amount {
		get => _amount;
		set {
			_amount = Mathf.Clamp(value, 0f, _maxAmount);

			const ulong damagedHealthDuration = (ulong)(1.25 * 1000);
			_damagedHealthTimer.SetDurationMSec(damagedHealthDuration);

			EmitSignal(SignalName.HealthChange, Amount);
		}
	}

	[Export] public float DamagedHealth {
		get => _damagedHealth;
		private set {}
	}


	[Signal] public delegate void HealthChangeEventHandler(float amount);



	public Health() : base() {
		Name = nameof(Health);
	}



	public override void _Ready() {
		base._Ready();

		_amount = _maxAmount;
		_damagedHealth = _amount;
	}

	public override void _Process(double delta) {
		base._Process(delta);

		if ( _damagedHealthTimer.IsDone ) {
			_damagedHealth = MathUtility.SmoothDamp(_damagedHealth, _amount, ref _damagedHealthVelocity, 0.2f, Mathf.Inf, (float)delta);
		} else {
			_damagedHealthVelocity = 0f;
		}
	}
}