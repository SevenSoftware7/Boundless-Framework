using System;
using Godot;


namespace SevenGame.Utility;

public struct TimeDuration {

    public float startTime = 0;
    public readonly float Duration => Time.GetTicksMsec() - startTime;



    public TimeDuration() {;}



    public void Start(){
        startTime = Time.GetTicksMsec();
    }


    public static implicit operator float(TimeDuration timer) => timer.Duration;

}