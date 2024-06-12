namespace LandlessSkies.Core;

using Godot;

public sealed partial class SlashAttack(Entity entity, SingleWeapon weapon) : Attack(entity, weapon) {
	public override bool IsCancellable => false;
	public override bool IsKnockable => true;



	public override void _Ready() {
		base._Ready();
		GD.Print($"{Weapon.DisplayName} Slash Attack");
		QueueFree();
	}
}