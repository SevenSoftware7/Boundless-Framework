namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public sealed partial class WeaponCostumeResourceItemKey : CostumeResourceItemKey {
	public WeaponCostumeResourceItemKey() : base() {}
	public WeaponCostumeResourceItemKey(string? key) : base(key) {}
}