using System;
using Godot;


namespace SevenGame.Utility;

public struct KeyInputInfo {

    private const float HOLD_TIME = 0.15f;


    public bool currentValue = false;
    public bool lastValue = false;

    public TimeDuration trueTimer = new();
    public TimeDuration falseTimer = new();

    public readonly bool Started => currentValue && !lastValue;
    public readonly bool Stopped => !currentValue && lastValue;
    private bool _updatedThisStep = false;



    public KeyInputInfo() {;}



    public static bool SimultaneousTap(KeyInputInfo a, KeyInputInfo b, float time = HOLD_TIME) {
        bool aTapped = a.trueTimer.Duration < time && b.Started;
        bool bTapped = b.trueTimer.Duration < time && a.Started;
        return aTapped || bTapped;
    }

    public readonly bool Tapped(float time = HOLD_TIME) => Stopped && trueTimer.Duration < time;
    public readonly bool Held(float time = HOLD_TIME) => currentValue && trueTimer.Duration > time;

    public readonly bool SimultaneousTap(KeyInputInfo other, float time = HOLD_TIME) {
        return SimultaneousTap(this, other, time);
    }
    
    public void SetVal(bool value) {
        if ( !_updatedThisStep ) {
            lastValue = currentValue;
        }
        currentValue = value;
        
        if (currentValue) {
            falseTimer.Start();
        } else {
            trueTimer.Start();
        }
    }

    public void TimeStep() {
        if ( !_updatedThisStep ) {
            SetVal(currentValue);
        }
        _updatedThisStep = false;
    }


    public static implicit operator bool(KeyInputInfo data) => data.currentValue;
}