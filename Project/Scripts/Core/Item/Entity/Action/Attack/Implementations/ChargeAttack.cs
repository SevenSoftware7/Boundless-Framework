namespace LandlessSkies.Core;

using SevenDev.Utility;

public abstract partial class ChargeAttack(Entity entity, SingleWeapon weapon, ulong chargeDuration = 1000) : Attack(entity, weapon), IPlayerHandler {
	public override bool IsCancellable => true;
	public override bool IsKnockable => true;

	private readonly TimeDuration chargeTime = new(chargeDuration);
	private bool isDone;



	protected abstract bool IsChargeStopped(InputDevice inputDevice);

	protected abstract void ChargeDone();
	protected abstract void ChargedAttack();
	protected abstract void UnchargedAttack();


	public virtual void HandlePlayer(Player player) {
		if (! isDone && chargeTime.IsDone) {
			isDone = true;
			ChargeDone();
		}

		if (IsChargeStopped(player.InputDevice)) {
			if (chargeTime.IsDone) {
				ChargedAttack();
			}
			else {
				UnchargedAttack();
			}

			QueueFree();
		}
	}
	public virtual void DisavowPlayer() { }
}