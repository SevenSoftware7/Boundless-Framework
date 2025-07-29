namespace Seven.Boundless;

using Godot;

public abstract partial class AttackDamageHitboxBehaviour : DamageHitboxBehaviour {
	[Export] public bool Attached = false;
	[Export] public Vector3 Offset = Vector3.Zero;

	public sealed override void Setup(DamageArea damageArea, CollisionShape3D collision) {
		if (damageArea.DamageDealer is not Attack attack) return;
		AttackSetup(attack, damageArea, collision);
	}

	protected abstract void AttackSetup(Attack attack, DamageArea damageArea, CollisionShape3D collision);
}