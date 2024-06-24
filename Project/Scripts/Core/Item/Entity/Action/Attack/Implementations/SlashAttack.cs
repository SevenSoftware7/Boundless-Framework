namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;

public sealed partial class SlashAttack(Entity entity, Weapon weapon, StringName library) : AnimationAttack(entity, weapon, library) {
	private static readonly StringName Attack = "Slash";

	protected override StringName AnimationName => Attack;
	public override bool IsCancellable => activeHitBoxes.Count == 0;


	public override DamageArea3D CreateHurtArea(float damage, ulong lifeTime) {
		return base.CreateHurtArea(damage, lifeTime);
	}

	protected override void _Start() {
		base._Start();
		GD.Print($"{Weapon.DisplayName} Slash Attack");
	}
}