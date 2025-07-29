using Godot;

namespace Seven.Boundless;

[Tool]
[GlobalClass]
public sealed partial class AttackDamageHitboxFollowWeaponBehaviour : AttackDamageHitboxBehaviour {
	[Export] public bool AttachToTip = false;

	protected override void AttackSetup(Attack attack, DamageArea damageArea, CollisionShape3D collision) {
		NodeAttacher attacher = new() {
			Follower = collision,
			TransformFunction = node => (
				AttachToTip
					? attack.Weapon.GetTipTransform()
					: attack.Weapon.GlobalTransform
			)
				.TranslatedLocal(Offset),
		};

		collision.AddChild(attacher);

		if (!Attached) {
			attacher.QueueFree();
		}
	}
}