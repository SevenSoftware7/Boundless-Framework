using Godot;
using System;


namespace LandlessSkies.Core;

[Tool]
// [GlobalClass]
public abstract partial class Costume : Resource {

    public abstract Model Instantiate(Node3D root, Skeleton3D skeleton);
}
