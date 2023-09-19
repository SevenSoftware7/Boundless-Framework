using System;
using Godot;


namespace SevenGame.Utility;

[Tool]
public partial class BoolInfo : RefCounted {
    [Export] public bool currentValue = false;
    [Export] public bool lastValue = false;

    [Export] public TimeDuration trueTimer = new();
    [Export] public TimeDuration falseTimer = new();

    [Export] public bool Started {
        get => currentValue && !lastValue;
        private set {;}
    }
    [Export] public bool Stopped {
        get => !currentValue && lastValue;
        private set {;}
    }

    public BoolInfo() : base() {;}


    public void SetVal(bool value) {
        if (currentValue) falseTimer.Start();
        else trueTimer.Start();

        lastValue = currentValue;
        currentValue = value;
    }



    public static implicit operator bool(BoolInfo data) => data.currentValue;
}