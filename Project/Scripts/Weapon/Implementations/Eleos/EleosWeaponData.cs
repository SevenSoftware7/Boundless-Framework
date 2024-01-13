using Godot;
using Godot.Collections;

namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class EleosWeaponData : WeaponData {
	public override EleosWeapon Instantiate(WeaponCostume? costume = null) =>
		new(this, costume);
}