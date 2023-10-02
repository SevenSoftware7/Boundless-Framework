using System;
using Godot;

using SevenGame.Utility;


namespace EndlessSkies.Core;

[Tool]
[GlobalClass]
public partial class Health : Node {

    // [Tooltip("The current health.")]

    // [Tooltip("The health, before it took damage. Slowly moves toward the true health.")]
    [Export] private float _damagedHealth;

    [Export] private float _damagedHealthVelocity = 0f;
    private TimeInterval _damagedHealthTimer = new();


    public event Action<float> OnUpdate;

    public delegate void HealthEvent(float amount);



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

            const float damagedHealthDuration = 1.25f;
            _damagedHealthTimer.SetDuration((ulong)damagedHealthDuration);

            OnUpdate?.Invoke(Amount);
        }
    }
    private float _amount;



    public override void _Ready() {
        base._Ready();

        _amount = _maxAmount;
        _damagedHealth = _amount;
    }

    public override void _Process(double delta) {
        base._Process(delta);

        // if ( _damagedHealthTimer.IsDone )
        //     _damagedHealth = VectorUtility.SmoothDamp(_damagedHealth, _amount, ref _damagedHealthVelocity, 0.2f, Mathf.Inf, (float)delta);
        // else {
        //     _damagedHealthVelocity = 0f;
        // }
    
    }
}
