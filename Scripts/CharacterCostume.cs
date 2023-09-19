using Godot;
using System;

namespace EndlessSkies.Core;

[Tool]
[GlobalClass]
public partial class CharacterCostume : Resource {

    [Export] public PackedScene ModelScene { get; private set; }
}
