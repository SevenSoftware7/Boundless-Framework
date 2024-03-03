

using System;
using Godot;

namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public abstract partial class WeaponData : Resource, IUIObject {

	[Export] public string DisplayName { get; private set; } = string.Empty;
	public Texture2D? DisplayPortrait => BaseCostume?.DisplayPortrait;

	[Export] public WeaponCostume? BaseCostume { get; private set; }

	[Export] public IWeapon.Type Type { get; protected set; }
	[Export] public IWeapon.Usage Usage { get; protected set; }
	[Export] public IWeapon.Size Size { get; protected set; }

#if TOOLS
	protected virtual bool EditableType => true;
	protected virtual bool EditableUsage => true;
	protected virtual bool EditableSize => true;
#endif



	public WeaponData() : base() {}



	public abstract SingleWeapon Instantiate(WeaponCostume? costume = null);
}