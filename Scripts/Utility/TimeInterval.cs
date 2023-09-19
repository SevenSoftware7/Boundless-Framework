using System;
using Godot;


namespace SevenGame.Utility;

[Tool]
public partial class TimeInterval : RefCounted {
    [Export] public ulong startTime = 0;
    [Export] public ulong stopTime = 0;

    [Export] public bool IsDone {
        get => Time.GetTicksMsec() >= stopTime;
        set {;}
    }
    [Export] public ulong Duration {
        get => stopTime - startTime;
        set {;}
    }
    [Export] public ulong RemainingDuration {
        get => IsDone ? 0 : stopTime - Time.GetTicksMsec();
        set {;}
    }
    [Export] public float Progress {
        get => IsDone ? 1f : (Time.GetTicksMsec() - startTime) / Duration;
        set {;}
    }

    public TimeInterval() : base() {;}


    public void SetTime(ulong timeMsec){
        startTime = Time.GetTicksMsec();
        stopTime = timeMsec;
    }
    public void SetDuration(ulong durationMsec){
        startTime = Time.GetTicksMsec();
        stopTime = startTime + durationMsec;
    }


    public static implicit operator float(TimeInterval timeUntil) => timeUntil.RemainingDuration;
}
