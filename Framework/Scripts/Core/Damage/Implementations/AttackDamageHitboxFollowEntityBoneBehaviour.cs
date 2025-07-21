namespace SevenDev.Boundless;

using Godot;
using SevenDev.Boundless.Utility;

[Tool]
[GlobalClass]
public sealed partial class AttackDamageHitboxFollowEntityBoneBehaviour : AttackDamageHitboxBehaviour {
	[Export] public string BoneName = "";

	protected override void AttackSetup(Attack attack, DamageArea damageArea, CollisionShape3D collision) {

		NodeAttacher attacher = new() {
			Follower = collision,
			TransformFunction = node => {
				Transform3D transform = attack.Entity.Skeleton.GetBoneTransformOrDefault(BoneName, attack.Entity.GlobalTransform);

				return transform.TranslatedLocal(Offset);
			},
		};

		collision.AddChild(attacher);

		if (!Attached) {
			attacher.QueueFree();
		}
	}
}