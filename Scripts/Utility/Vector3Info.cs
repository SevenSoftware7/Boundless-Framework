// using System;
// using Godot;


// namespace SevenGame.Utility;

// [Tool]
// public partial class Vector3Info : RefCounted {

//     [Export] public Vector3 currentValue = Vector3.Zero;
//     [Export] public Vector3 lastValue = Vector3.Zero;
    
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
//     public float Z {
//         get => currentValue.Z;
//         private set {;}
//     }

//     public Vector3Info() : base() {;}


//     public float LengthSquared() => currentValue.LengthSquared();
//     public float Length() => currentValue.Length();
//     public Vector3 Normalized() => currentValue.Normalized();

//     public void SetVal(Vector3 value){
//         lastValue = currentValue;
//         currentValue = value;
        
//         if ( LengthSquared() == 0 ) {
//             nonZeroTimer.Start();
//         } else {
//             zeroTimer.Start();
//         }
//     }


//     public static implicit operator Vector3(Vector3Info data) => data.currentValue;
//     public static Vector3 operator *(Vector3Info a, float b) => a.currentValue * b;
//     public static Vector3 operator *(float a, Vector3Info b) => a * b.currentValue;
    
// }