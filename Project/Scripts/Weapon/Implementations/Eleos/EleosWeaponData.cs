using Godot;

namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class EleosWeaponData : WeaponData {

	private EleosWeaponData() : base() {}



	public override EleosWeapon Instantiate(WeaponCostume? costume = null) =>
		new(this, costume);
}