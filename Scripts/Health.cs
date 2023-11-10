using System;
using Godot;

using SevenGame.Utility;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class Health : Node {

    [Export] public float MaxAmount {
        get => _maxAmount;
        set {
            _maxAmount = Mathf.Max(value, 1f);
            _damagedHealth = Mathf.Min(_damagedHealth, _maxAmount);
        }
    }
    private float _maxAmount = 100f;

    [Export] public float Amount {
        get {
            _amount = Mathf.Clamp(_amount, 0f, _maxAmount);
            return _amount;
        }
        set {
            _amount = value;

            const ulong damagedHealthDuration = (ulong)(1.25 * 1000);
            _damagedHealthTimer.SetDurationMSec(damagedHealthDuration);

            EmitSignal(SignalName.HealthChange, Amount);
        }
    }
    private float _amount;

    [Export] public float DamagedHealth {
        get => _damagedHealth;
        private set => _damagedHealth = value;
    }
    private float _damagedHealth;

    private TimeInterval _damagedHealthTimer = new();
    [Export] private float _damagedHealthVelocity = 0f;


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
