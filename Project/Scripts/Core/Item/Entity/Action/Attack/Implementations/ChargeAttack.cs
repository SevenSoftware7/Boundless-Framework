namespace LandlessSkies.Core;

using System.Collections.Generic;
public abstract partial class ChargeAttack(Entity entity, Weapon weapon, IEnumerable<AttributeModifier>? modifiers = null) : Attack(entity, weapon, modifiers), IPlayerHandler {
	public override bool IsCancellable => true;
	public override bool IsInterruptable => true;

	protected abstract bool IsChargeStopped(InputDevice inputDevice);

	protected abstract void _Attack();


	public virtual void HandlePlayer(Player player) {
		if (IsChargeStopped(player.InputDevice)) {
			_Attack();

			Stop();
		}
	}
	public virtual void DisavowPlayer() { }
}