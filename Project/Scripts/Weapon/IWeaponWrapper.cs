using Godot;
using Godot.Collections;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class IWeaponWrapper : InterfaceWrapper<IWeapon> {

    public override string HintString => IWeaponInfo.HintString;
}