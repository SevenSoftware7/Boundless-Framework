using System;
using Godot;


namespace SevenGame.Utility;

public struct Vector3Info {

    public Vector3 currentValue = Vector3.Zero;
    public Vector3 lastValue = Vector3.Zero;
    
    public TimeDuration zeroTimer = new();
    public TimeDuration nonZeroTimer = new();

    public readonly float X => currentValue.X;
    public readonly float Y => currentValue.Y;
    public readonly float Z => currentValue.Z;
    private bool _updatedThisStep = false;



    public Vector3Info() {;}



    public readonly float LengthSquared() => currentValue.LengthSquared();
    public readonly float Length() => currentValue.Length();
    public readonly Vector3 Normalized() => currentValue.Normalized();


    public void SetVal(Vector3 value) {
        if ( ! _updatedThisStep ) {
            lastValue = currentValue;
        }
        currentValue = value;
        
        if ( LengthSquared() == 0 ) {
            nonZeroTimer.Start();
        } else {
            zeroTimer.Start();
        }
    }

    public void TimeStep() {
        if ( ! _updatedThisStep ) {
            SetVal(Vector3.Zero);
        }
        _updatedThisStep = false;
    }


    public static implicit operator Vector3(Vector3Info data) => data.currentValue;
    public static Vector3 operator *(Vector3Info a, float b) => a.currentValue * b;
    public static Vector3 operator *(float a, Vector3Info b) => a * b.currentValue;
    
}