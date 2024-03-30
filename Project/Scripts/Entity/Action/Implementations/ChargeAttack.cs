namespace LandlessSkies.Core;

using Godot;
using SevenGame.Utility;

public abstract partial class ChargeAttack : Attack {
	public override bool IsCancellable => true;
	public override bool IsKnockable => true;

	public abstract ulong ChargeDuration { get; }

	private TimeDuration chargeTime;
	private bool isDone = false;



	private ChargeAttack() : this(null!) {}
	public ChargeAttack(SingleWeapon weapon) : base(weapon) {
		chargeTime = new(ChargeDuration);
	}



	protected abstract bool IsChargeStopped(InputDevice inputDevice);

	protected abstract void ChargeDone();
	protected abstract void ChargedAttack();
	protected abstract void UnchargedAttack();

	public override void _Process(double delta) {
		base._Process(delta);

		if (! isDone && chargeTime.IsDone) {
			isDone = true;
			ChargeDone();
		}
	}

	public override void HandleInput(CameraController3D cameraController, InputDevice inputDevice) {
		base.HandleInput(cameraController, inputDevice);

		if (IsChargeStopped(inputDevice)) {
			if (chargeTime.IsDone) {
				ChargedAttack();
			} else {
				UnchargedAttack();
			}

			QueueFree();
		}
	}
}