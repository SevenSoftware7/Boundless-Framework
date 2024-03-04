using Godot;
using Godot.Collections;
using System;


namespace LandlessSkies.Core;

[Tool]
public abstract partial class WeaponModel : Model {
	public override WeaponCostume Costume => (_costume as WeaponCostume)!;


	protected WeaponModel() : base() {}
	public WeaponModel(WeaponCostume costume) : base(costume) {}
}