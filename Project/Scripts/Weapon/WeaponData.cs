

using System;
using Godot;

namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class WeaponData : Resource, IUIObject {
	
	[Export] public string DisplayName { get; private set; } = string.Empty;
	public Texture2D? DisplayPortrait => BaseCostume?.DisplayPortrait;

	[Export] public WeaponCostume? BaseCostume { get; private set; }

	[Export] public IWeapon.Type Type { get; private set; }



	public WeaponData() : base() {}



	public virtual SingleWeapon Instantiate(WeaponCostume? costume = null) =>
		new(this, costume);
}