namespace LandlessSkies.Core;

using Godot;
using SevenDev.Boundless.Utility;

public partial class BipedJumpAction : JumpAction, IPlayerHandler {
	public const float INITIAL_JUMP_HEIGHT_FRACTION = 2f / 3f;

	private float maxDistance = EntityStats.DEFAULT_JUMP_HEIGHT * (1f - INITIAL_JUMP_HEIGHT_FRACTION);
	private float remainingDistance = 1f;

	public override bool IsCancellable => true;
	public override bool IsInterruptable => true;

	public BipedJumpAction(Entity entity, Vector3 direction) : base(entity) {
		Direction = direction;
	}


	public override void _Process(double delta) {
		base._Process(delta);

		if (remainingDistance <= 0 || Entity.IsOnFloor()) {
			Stop();
		}

		if (maxDistance != 0) {
			float travelDistance = Mathf.Min(maxDistance * (float)delta * 5f, remainingDistance * maxDistance);
			Entity.Inertia += Direction * travelDistance;
			remainingDistance -= travelDistance / maxDistance;
		}
	}


	public void HandlePlayer(Player player) {
		if (!player.InputDevice.IsActionPressed(Inputs.Jump)) {
			Stop();
			return;
		}
	}

	public void DisavowPlayer() { }

	protected override void _Start() {
		float jumpHeight = Entity.AttributeModifiers.ApplyTo(Attributes.GenericjumpHeight, Entity.Stats.JumpHeight);

		float initialJumpHeight = jumpHeight * INITIAL_JUMP_HEIGHT_FRACTION;
		Entity.Inertia = Entity.Inertia.SlideOnFace(Direction) + Direction * initialJumpHeight;
		maxDistance = jumpHeight - initialJumpHeight;
	}

	protected override void _Stop() { }



	public new class Builder : JumpAction.Builder {
		public Vector3? Direction;



		public override JumpAction Build(Entity entity) => new BipedJumpAction(entity, Direction ?? entity.UpDirection);
	}
}