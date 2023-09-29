using System;
using Godot;


namespace SevenGame.Utility;

public struct BoolInfo {
    public bool currentValue = false;
    public bool lastValue = false;

    public TimeDuration trueTimer = new();
    public TimeDuration falseTimer = new();

    public readonly bool Started => currentValue && !lastValue;
    public readonly bool Stopped => !currentValue && lastValue;

    private bool _updatedThisStep = false;



    public BoolInfo() {;}



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



    public static implicit operator bool(BoolInfo data) => data.currentValue;
}