using Godot;
using System;
using System.Diagnostics;
using CharacterEmotion = LandlessSkies.Core.IPortraitProvider.CharacterEmotion;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class WeaponCostume : Costume {

    [Export] public PackedScene ModelScene { get; private set; }

    public override WeaponModel Instantiate(Node3D root, Skeleton3D skeleton) {
        return new WeaponModel(root, skeleton, this);
    }

}
