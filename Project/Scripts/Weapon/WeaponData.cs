

using Godot;

namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class WeaponData : Resource, IUIObject {
	
	[Export] public string DisplayName { get; private set; } = "";
	public Texture2D? DisplayPortrait => BaseCostume?.DisplayPortrait;

	[Export] public WeaponCostume? BaseCostume { get; private set; }

	[Export] public IWeapon.Type Type { get; private set; }



	public WeaponData() : base() {}



	public virtual Weapon Instantiate(Node3D root, WeaponCostume? costume = null) {
		return new SingleWeapon(this, costume ?? BaseCostume, root);
	}
}