namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;

public abstract partial class ChargeAttack : Attack {
	public override bool IsCancellable => true;
	public override bool IsKnockable => true;

	public abstract ulong ChargeDuration { get; }

	private TimeDuration chargeTime;
	private bool isDone;



	private ChargeAttack() : this(null!) { }
	public ChargeAttack(SingleWeapon weapon) : base(weapon) {
		chargeTime = new(ChargeDuration);
	}



	protected abstract bool IsChargeStopped(InputDevice inputDevice);

	protected abstract void ChargeDone(Entity entity);
	protected abstract void ChargedAttack(Entity entity);
	protected abstract void UnchargedAttack(Entity entity);


	public override void HandlePlayer(Player player) {
		base.HandlePlayer(player);

		if (!isDone && chargeTime.IsDone) {
			isDone = true;
			ChargeDone(player.Entity);
		}

		if (IsChargeStopped(player.InputDevice)) {
			if (chargeTime.IsDone) {
				ChargedAttack(player.Entity);
			}
			else {
				UnchargedAttack(player.Entity);
			}

			QueueFree();
		}
	}
}