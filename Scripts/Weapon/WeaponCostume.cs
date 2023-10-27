using Godot;


namespace LandlessSkies.Core;

[Tool]
// [GlobalClass]
public abstract partial class WeaponCostume : Costume {

    public override abstract WeaponModel Instantiate(Node3D root, Skeleton3D? skeleton);

}
