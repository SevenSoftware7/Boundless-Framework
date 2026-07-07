use crate::{damage::{DamageDealer, DamageInstance, Damageable}, id::Id, sync::{BdlsMutex, BdlsPtr}};

pub trait DamageModifier {
	fn apply(
		&self,
		damage: BdlsPtr<BdlsMutex<DamageInstance>>
	) {
		let _ = damage;
	}

	fn add_effects(
		&self,
		damage: BdlsPtr<BdlsMutex<DamageInstance>>
	) {
		let _ = damage;
	}
}


pub fn scale_damage(
	base_amount: f32,
	resistance_attribute: &Id,
	strength_attribute: &Id,
	target: &dyn Damageable,
	damage_dealer: Option<&dyn DamageDealer>,
	allow_negative: bool
) -> f32 {
	let mut modified_amount: f32 = base_amount;

	if let Some(dealer) = damage_dealer {
		let strength_value = dealer.get_attribute(strength_attribute).unwrap_or(1.0);
		modified_amount *= strength_value;
	}

	let resistance_value = target.get_attribute(resistance_attribute).unwrap_or(1.0);
	if resistance_value == 0.0 {
		modified_amount = f32::INFINITY;
	} else {
		modified_amount /= resistance_value;
	}

	if allow_negative {
		modified_amount
	} else {
		modified_amount.max(0.0)
	}
}