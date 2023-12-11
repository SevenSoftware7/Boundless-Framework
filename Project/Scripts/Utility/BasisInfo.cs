using System;
using Godot;


namespace SevenGame.Utility;

public struct BasisInfo {
    
    [Export] public Basis currentValue = Basis.Identity;
    [Export] public Basis lastValue = Basis.Identity;
    
    public readonly Vector3 X => currentValue.X;
    public readonly Vector3 Y => currentValue.Y;
    public readonly Vector3 Z => currentValue.Z;



    public BasisInfo() {;}



    public void SetVal(Basis value) {
        currentValue = value;
    }

    public static implicit operator Basis(BasisInfo data) => data.currentValue;
    public static Vector3 operator *(BasisInfo a, Vector3 b) => a.currentValue * b;
    public static Vector3 operator *(BasisInfo a, Vector3Info b) => a.currentValue * b.currentValue;
    public static Basis operator *(BasisInfo a, BasisInfo b) => a.currentValue * b.currentValue;
    
}