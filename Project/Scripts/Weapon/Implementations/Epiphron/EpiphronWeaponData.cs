using Godot;

namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class EpiphronWeaponData : WeaponData {
#if TOOLS
	protected override bool EditableType => false;
	protected override bool EditableUsage => false;
	protected override bool EditableSize => false;
#endif


	private EpiphronWeaponData() : base() {
		Type = IWeapon.Type.Sword;
		Usage = IWeapon.Usage.Slash | IWeapon.Usage.Thrust;
		Size = IWeapon.Size.OneHanded | IWeapon.Size.TwoHanded;
	}



	public override EpiphronWeapon Instantiate(WeaponCostume? costume = null) =>
		new(this, costume);
}