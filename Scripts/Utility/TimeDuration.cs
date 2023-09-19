using System;
using Godot;


namespace SevenGame.Utility;

[Tool]
public partial class TimeDuration : RefCounted {
    [Export] public float startTime = 0;
    
    [Export] public float Duration {
        get => Time.GetTicksMsec() - startTime;
        set {;}
    }

    public TimeDuration() : base() {;}


    public void Start(){
        startTime = Time.GetTicksMsec();
    }


    public static implicit operator float(TimeDuration timer) => timer.Duration;

}