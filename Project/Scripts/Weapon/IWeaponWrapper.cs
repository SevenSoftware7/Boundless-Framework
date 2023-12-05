using Godot;
using Godot.Collections;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class IWeaponWrapper : InterfaceWrapper, IInterfaceWrapper<IWeapon> {

    public override string HintString => IWeaponInfo.HintString;
    public IWeapon? Get(Node root) => Get<IWeapon>(root);
    public void Set(Node root, IWeapon? value) => Set<IWeapon>(root, value);
}