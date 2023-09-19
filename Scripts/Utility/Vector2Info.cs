// using System;
// using Godot;


// namespace SevenGame.Utility;

// [Tool]
// public partial class Vector2Info : RefCounted {

//     [Export] public Vector2 currentValue = Vector2.Zero;
//     [Export] public Vector2 lastValue = Vector2.Zero;
    
//     [Export] public TimeDuration zeroTimer = new();
//     [Export] public TimeDuration nonZeroTimer = new();

//     public float X {
//         get => currentValue.X;
//         private set {;}
//     }
//     public float Y {
//         get => currentValue.Y;
//         private set {;}
//     }

//     public Vector2Info() : base() {;}


//     public float LengthSquared() => currentValue.LengthSquared();
//     public float Length() => currentValue.Length();
//     public Vector2 Normalized() => currentValue.Normalized();
    
//     public void SetVal(Vector2 value){
//         lastValue = currentValue;
//         currentValue = value;
        
//         if ( LengthSquared() == 0 ) {
//             nonZeroTimer.Start();
//         } else {
//             zeroTimer.Start();
//         }
//     }


//     public static implicit operator Vector2(Vector2Info data) => data.currentValue;
//     public static Vector2 operator *(Vector2Info a, float b) => a.currentValue * b;
//     public static Vector2 operator *(float a, Vector2Info b) => a * b.currentValue;
// }