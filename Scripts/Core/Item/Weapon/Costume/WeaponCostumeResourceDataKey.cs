namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public sealed partial class WeaponCostumeResourceDataKey : CostumeResourceDataKey {
	public WeaponCostumeResourceDataKey() : base() {}
	public WeaponCostumeResourceDataKey(string? key) : base(key) {}
}