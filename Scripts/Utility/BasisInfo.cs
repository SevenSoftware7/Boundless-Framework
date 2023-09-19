// using System;
// using Godot;


// namespace SevenGame.Utility;

// [Tool]
// public partial class BasisInfo : RefCounted {
    
//     [Export] public Basis currentValue = Basis.Identity;
//     [Export] public Basis lastValue = Basis.Identity;
    
//     /* [Export]  */public Vector3 X {
//         get => currentValue.X;
//         private set {;}
//     }
//     /* [Export]  */public Vector3 Y {
//         get => currentValue.Y;
//         private set {;}
//     }
//     /* [Export]  */public Vector3 Z {
//         get => currentValue.Z;
//         private set {;}
//     }

//     public BasisInfo() : base() {;}


//     public void SetVal(Basis value) { 
//         lastValue = currentValue;
//         currentValue = value;
//     }


//     public static implicit operator Basis(BasisInfo data) => data.currentValue;
//     public static Vector3 operator *(BasisInfo a, Vector3 b) => a.currentValue * b;
//     public static Vector3 operator *(BasisInfo a, Vector3Info b) => a.currentValue * b.currentValue;
//     public static Basis operator *(BasisInfo a, BasisInfo b) => a.currentValue * b.currentValue;
    
// }