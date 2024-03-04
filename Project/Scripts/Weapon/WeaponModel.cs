using Godot;

namespace LandlessSkies.Core;

[Tool]
public abstract partial class WeaponModel : Model {
	public new WeaponCostume Costume {
		get => (base.Costume as WeaponCostume)!;
		set => base.Costume = value;
	}


	protected WeaponModel() : base() {}
	public WeaponModel(WeaponCostume costume) : base(costume) {}
}