namespace SevenDev.Boundless;

using Godot;

[Tool]
[GlobalClass]
public sealed partial class AttackDamageHitboxFollowEntityCenterOfMass : AttackDamageHitboxBehaviour {
	[Export] public bool AttachToCenterOfMass = false;

	protected override void AttackSetup(Attack attack, DamageArea damageArea, CollisionShape3D collision) {
		NodeAttacher attacher = new() {
			Follower = collision,
			TransformFunction = node => (
				AttachToCenterOfMass && attack.Entity.CenterOfMass is not null
					? attack.Entity.CenterOfMass.GlobalTransform
					: attack.Entity.GlobalTransform
			)
				.TranslatedLocal(Offset),
		};

		collision.AddChild(attacher);

		if (!Attached) {
			attacher.QueueFree();
		}
	}
}