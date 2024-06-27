namespace LandlessSkies.Core;

using Godot;

public sealed partial class SlashAttack(Entity entity, Weapon weapon, StringName library) : AnimationAttack(entity, weapon, library) {
	private static readonly AttributeModifier slowRotation = new MultiplicativeModifier(Attributes.GenericTurnSpeed, 0.175f);
	private static readonly AttributeModifier slowMovement = new MultiplicativeModifier(Attributes.GenericMoveSpeed, 0.175f);
	private static readonly AttributeModifier lowGravity = new MultiplicativeModifier(Attributes.GenericGravity, 0.025f);
	private static readonly StringName Attack = "Slash";

	protected override StringName AnimationName => Attack;


	public override DamageArea3D CreateHurtArea(float damage, ulong lifeTime) {
		return base.CreateHurtArea(damage, lifeTime);
	}

	protected override void _Start() {
		base._Start();
		Entity.AttributeModifiers.Add(slowRotation);
		Entity.AttributeModifiers.Add(slowMovement);
		Entity.AttributeModifiers.Add(lowGravity);
		GD.Print($"{Weapon.DisplayName} Slash Attack");
	}

	protected override void _Stop() {
		base._Stop();
		Entity.AttributeModifiers.Remove(slowRotation);
		Entity.AttributeModifiers.Remove(slowMovement);
		Entity.AttributeModifiers.Remove(lowGravity);
	}
}