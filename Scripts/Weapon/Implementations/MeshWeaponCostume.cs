using Godot;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public sealed partial class MeshWeaponCostume : WeaponCostume {

    [Export] public PackedScene? ModelScene { get; private set; }

    public override WeaponModel Instantiate() {
        return new MeshWeaponModel(this);
    }
}