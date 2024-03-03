using Godot;

namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class EleosWeaponData : WeaponData {
#if TOOLS
    protected override bool EditableType => false;
    protected override bool EditableUsage => false;
    protected override bool EditableSize => false;
#endif


    private EleosWeaponData() : base() {
		Type = IWeapon.Type.Sword;
		Usage = IWeapon.Usage.Slashing | IWeapon.Usage.Thrusting;
		Size = IWeapon.Size.OneHanded;
	}



	public override EleosWeapon Instantiate(WeaponCostume? costume = null) =>
		new(this, costume);
}