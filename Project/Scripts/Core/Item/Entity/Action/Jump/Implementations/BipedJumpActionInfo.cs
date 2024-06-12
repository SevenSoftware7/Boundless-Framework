using Godot;

namespace LandlessSkies.Core;

public partial class BipedJumpActionInfo : JumpActionInfo {
	private static readonly float PotentialJumpHeight = 1f + BipedJumpAction.JUMP_HEIGHT_FRACTION;
	public override float PotentialHeight => PotentialJumpHeight;


	protected internal override JumpAction Create(Entity entity, Vector3 direction) {
		return new BipedJumpAction(entity, direction);
	}
}