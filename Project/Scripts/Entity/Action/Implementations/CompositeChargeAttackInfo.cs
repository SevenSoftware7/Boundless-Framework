namespace LandlessSkies.Core;

using Godot;

public record class CompositeChargeAttackInfo(SingleWeapon Weapon, AttackInfo UnchargedAttack, AttackInfo ChargedAttack) : ChargeAttackInfo(Weapon) {
	public StringName ActionKey { get; private set; } = "attack_light";
	public ulong ChargeDuration { get; private set; } = 1000;
	public AttackInfo UnchargedAttack { get; private set; } = UnchargedAttack;
	public AttackInfo ChargedAttack { get; private set; } = ChargedAttack;



	public CompositeChargeAttackInfo(
		SingleWeapon weapon,
		AttackInfo unchargedAttack,
		AttackInfo chargedAttack,
		string actionKey = "attack_light",
		ulong chargeDuration = 1000
	) : this(weapon, unchargedAttack, chargedAttack) {
		(ActionKey, ChargeDuration) = (actionKey, chargeDuration);
	}



	public override CompositeChargeAttack Build() {
		return new CompositeChargeAttack(Weapon, this);
	}
}