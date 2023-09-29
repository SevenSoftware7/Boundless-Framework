using Godot;
using System;


namespace EndlessSkies.Core;

[Tool]
// [GlobalClass]
public abstract partial class Costume : Resource {

    public abstract Model CreateModel(IModelAttachment attachment);
}
