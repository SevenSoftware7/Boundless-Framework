using Godot;

namespace SevenDev.Boundless;

[GlobalClass]
public abstract partial class DamageHitboxBehaviour : Resource {
	/// <summary>
	/// Called when the hitbox is set up.
	/// </summary>
	/// <param name="damageArea">The damage area to which the hitbox belongs.</param>
	/// <param name="collision">The collision shape of the hitbox.</param>
	public abstract void Setup(DamageArea damageArea, CollisionShape3D collision);
}