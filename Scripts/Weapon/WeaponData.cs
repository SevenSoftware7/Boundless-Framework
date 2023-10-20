

using Godot;

namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class WeaponData : Resource {

    [Export] public WeaponCostume BaseCostume { get; private set; }

    [Export] public IWeapon.Handedness Handedness { get; private set; }
    [Export] public IWeapon.Type Type { get; private set; }


    public WeaponData() : base() {;}


    public virtual Weapon Instantiate(Node3D root, Skeleton3D armature) {
        return new Weapon(root, armature, this);
    }

}