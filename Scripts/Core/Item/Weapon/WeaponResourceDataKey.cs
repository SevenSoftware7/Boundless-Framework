namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public sealed partial class WeaponResourceDataKey : GenericResourceDataKey<Weapon> {
	public WeaponResourceDataKey() : base() {}
	public WeaponResourceDataKey(string? key) : base(key) {}
}