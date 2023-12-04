using System;
using Godot;


namespace SevenGame.Utility;

public struct TimeInterval {
    public ulong startTime = 0;
    public ulong stopTime = 0;

    public readonly bool IsDone => Time.GetTicksMsec() >= stopTime;
    public readonly ulong Duration => stopTime - startTime;
    public readonly ulong RemainingDuration => IsDone ? 0 : stopTime - Time.GetTicksMsec();
    public readonly float Progress => IsDone ? 1f : (Time.GetTicksMsec() - startTime) / Duration;



    public TimeInterval() {;}



    public void SetTime(ulong timeMsec){
        startTime = Time.GetTicksMsec();
        stopTime = timeMsec;
    }
    public void SetDurationMSec(ulong durationMsec){
        startTime = Time.GetTicksMsec();
        stopTime = startTime + durationMsec;
    }


    public static implicit operator float(TimeInterval timeUntil) => timeUntil.RemainingDuration;
}
