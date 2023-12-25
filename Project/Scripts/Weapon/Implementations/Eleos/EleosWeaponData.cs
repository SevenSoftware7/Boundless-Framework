using Godot;
using Godot.Collections;

namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class EleosWeaponData : WeaponData {
	public override Weapon Instantiate(Node3D root, WeaponCostume? costume = null) {
		return new EleosWeapon(this, costume ?? BaseCostume, root);
	}
}