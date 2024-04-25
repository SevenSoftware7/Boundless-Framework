namespace LandlessSkies.Core;

using Godot;

public sealed partial class SlashAttack(SingleWeapon weapon) : Attack(weapon) {
	public override bool IsCancellable => false;
	public override bool IsKnockable => true;


	private SlashAttack() : this(null!) { }


	public override void _Ready() {
		base._Ready();
		GD.Print($"{Weapon.WeaponData.DisplayName} Slash Attack");
		QueueFree();
	}
}