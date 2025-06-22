namespace SevenDev.Boundless;

using Godot;
using SevenDev.Boundless.Utility;

public partial class BipedJumpAction : JumpAction, IPlayerHandler {
	public const float INITIAL_JUMP_HEIGHT_FRACTION = 4f / 5f;

	private float maxDistance = 20f * (1f - INITIAL_JUMP_HEIGHT_FRACTION);
	private float remainingDistance = 1f;

	public override bool IsCancellable => true;
	public override bool IsInterruptable => true;

	public BipedJumpAction(Entity entity, Builder builder, Vector3 direction) : base(entity, builder) {
		Direction = direction;
	}


	public override void _Process(double delta) {
		base._Process(delta);

		if (maxDistance == 0 || remainingDistance <= 0 || Entity.IsOnFloor()) {
			Stop();
			return;
		}

		float travelDistance = Mathf.Min(maxDistance * (float)delta * 5f, remainingDistance * maxDistance);
		Entity.Gravity += Direction * travelDistance;
		remainingDistance -= travelDistance / maxDistance;
	}


	public void HandlePlayer(Player player) {
		if (!player.InputDevice.IsActionPressed(Inputs.Jump)) {
			Stop();
			return;
		}
	}

	public void DisavowPlayer() { }

	protected override void _Start() {
		float jumpHeight = Entity.GetTraitValue(Traits.GenericJumpHeight);

		float initialJumpHeight = jumpHeight * INITIAL_JUMP_HEIGHT_FRACTION;
		Entity.Gravity = Entity.Gravity.SlideOnFace(Direction) + Direction * initialJumpHeight;
		maxDistance = jumpHeight - initialJumpHeight;
	}

	protected override void _Stop() { }



	public new sealed class Builder : JumpAction.Builder {
		public Vector3? Direction;



		public override JumpAction Build(Entity entity) => new BipedJumpAction(entity, this, Direction ?? entity.UpDirection);
	}
}