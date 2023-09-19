// using System;
// using Godot;


// namespace SevenGame.Utility;

// [Tool]
// public partial class KeyInputInfo : RefCounted {

//     private const float HOLD_TIME = 0.15f;


//     [Export] public bool currentValue = false;
//     [Export] public bool lastValue = false;

//     [Export] public TimeDuration trueTimer = new();
//     [Export] public TimeDuration falseTimer = new();

//     [Export] public bool Started {
//         get => currentValue && !lastValue;
//         private set {;}
//     }
//     [Export] public bool Stopped {
//         get => !currentValue && lastValue;
//         private set {;}
//     }

//     public KeyInputInfo() : base() {;}


//     public static bool SimultaneousTap(KeyInputInfo a, KeyInputInfo b, float time = HOLD_TIME) {
//         bool aTapped = a.trueTimer.Duration < time && b.Started;
//         bool bTapped = b.trueTimer.Duration < time && a.Started;
//         return aTapped || bTapped;
//     }

//     public bool Tapped(float time = HOLD_TIME) => Stopped && trueTimer.Duration < time;
//     public bool Held(float time = HOLD_TIME) => currentValue && trueTimer.Duration > time;

//     public bool SimultaneousTap(KeyInputInfo other, float time = HOLD_TIME) {
//         return SimultaneousTap(this, other, time);
//     }
    
//     public void SetVal(bool value){
//         if (currentValue) {
//             falseTimer.Start();
//         } else {
//             trueTimer.Start();
//         }

//         lastValue = currentValue;
//         currentValue = value;
//     }


//     public static implicit operator bool(KeyInputInfo data) => data.currentValue;
// }