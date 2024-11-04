namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public partial class SceneWeaponData : SceneItemData<Weapon> {
	public override IDataKeyProvider<Weapon> KeyProvider => _keyProvider;
	[Export] private WeaponResourceDataKey _keyProvider = new();
}