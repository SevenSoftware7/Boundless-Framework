namespace LandlessSkies.Core;

using SevenDev.Utility;

public abstract partial class ChargeAttack(SingleWeapon weapon, ulong chargeDuration = 1000) : Attack(weapon) {
	public override bool IsCancellable => true;
	public override bool IsKnockable => true;

	private readonly TimeDuration chargeTime = new(chargeDuration);
	private bool isDone;



	protected abstract bool IsChargeStopped(InputDevice inputDevice);

	protected abstract void ChargeDone(Entity entity);
	protected abstract void ChargedAttack(Entity entity);
	protected abstract void UnchargedAttack(Entity entity);


	public override void HandlePlayer(Player player) {
		base.HandlePlayer(player);

		if (player.Entity is null) return;

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