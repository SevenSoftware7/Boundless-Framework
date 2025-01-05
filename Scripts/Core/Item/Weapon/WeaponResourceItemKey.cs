namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public sealed partial class WeaponResourceItemKey : GenericResourceItemKey<Weapon> {
	public WeaponResourceItemKey() : base() {}
	public WeaponResourceItemKey(string? key) : base(key) {}
}