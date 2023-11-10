using Godot;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public abstract partial class WeaponCostume : Resource {

    public abstract WeaponModel Instantiate();

}
