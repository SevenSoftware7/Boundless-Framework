namespace Seven.Boundless;

using System.Collections.Generic;
using Seven.Boundless.Utility;



/// <summary>
/// A Charge Attack is an attack that can be charged an indeterminate amount of time.
/// </summary>
/// <param name="entity">Inherited from <see cref="EntityAction"/>.</param>
/// <param name="weapon">Inherited from <see cref="Attack"/>.</param>
/// <param name="modifiers">Inherited from <see cref="EntityAction"/>.</param>
public abstract partial class ChargeAttack : Attack, IPlayerHandler {
	public override bool IsCancellable => true;
	public override bool IsInterruptable => true;


	public ChargeAttack(Entity entity, Builder builder, Weapon weapon, AnimationPath path, IEnumerable<TraitModifier>? modifiers = null) : base(entity, builder, weapon, path, modifiers) {}


	/// <summary>
	/// Check whether the Charge should stop this frame.
	/// </summary>
	/// <param name="inputDevice">The Input device responsible for the Charge input</param>
	/// <returns>Whether the Charge should be stopped this frame.</returns>
	protected abstract bool IsChargeStopped(InputDevice inputDevice);

	/// <summary>
	/// Callback Method for when the Attack stops charging, i.e. when the attack is properly launched.
	/// </summary>
	protected abstract void _Attack();


	public virtual void HandlePlayer(Player player) {
		if (IsChargeStopped(player.InputDevice)) {
			_Attack();

			Stop();
		}
	}
	public virtual void DisavowPlayer() { }



	public new abstract class Builder(Weapon weapon) : Attack.Builder(weapon) {
		public abstract override ChargeAttack Build(Entity entity);
	}
}