using Godot;

namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
// TODO: when Interface reference [Export] is implemented in Godot, turn this into an interface
public abstract partial class WeaponCostume : Costume {
	public abstract override WeaponModel Instantiate();
}