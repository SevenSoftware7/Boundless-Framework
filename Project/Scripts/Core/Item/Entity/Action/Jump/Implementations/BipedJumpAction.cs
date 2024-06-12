using Godot;
using SevenDev.Utility;

namespace LandlessSkies.Core;

public partial class BipedJumpAction : JumpAction, IPlayerHandler {
	public const float JUMP_HEIGHT_FRACTION = 0.35f;
	private float maxDistance = EntityStats.DEFAULT_JUMP_HEIGHT * JUMP_HEIGHT_FRACTION;
	private float remainingDistance = 1f;

	public override bool IsCancellable => true;
	public override bool IsKnockable => true;

	public BipedJumpAction(Entity entity, Vector3 direction) : base(entity) {
		Direction = direction;
		float jumpHeight = Entity.AttributeModifiers.Get(Attributes.GenericjumpHeight).ApplyTo(Entity.Stats.JumpHeight);

		Entity.Inertia = Entity.Inertia.SlideOnFace(Direction) + Direction * jumpHeight;
		maxDistance = jumpHeight * JUMP_HEIGHT_FRACTION;
	}


	public void HandlePlayer(Player player) {
		if (! player.InputDevice.IsActionPressed(Inputs.Jump)) {
			QueueFree();
			return;
		}
	}

	public void DisavowPlayer() { }

	public override void _Process(double delta) {
		base._Process(delta);

		if (remainingDistance <= 0 || Entity.IsOnFloor()) {
			QueueFree();
		}

		if (Lifetime > 150) {
			float travelDistance = Mathf.Min(maxDistance * (float)delta * 5f, remainingDistance * maxDistance);
			Entity.Inertia += Direction * travelDistance;
			remainingDistance -= travelDistance / maxDistance;
		}
	}
}