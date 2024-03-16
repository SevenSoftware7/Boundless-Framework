namespace LandlessSkies.Core;

using Godot;

// TODO: when Interface reference [Export] is implemented in Godot, turn this into an interface
[Tool]
[GlobalClass]
public abstract partial class WeaponCostume : Costume {
	public abstract override WeaponModel Instantiate();
}