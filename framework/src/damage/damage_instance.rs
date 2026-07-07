use std::slice::Iter;

use crate::damage::scale_damage;
use crate::sync::{BdlsMutex, BdlsPtr, Lockable};
use crate::{damage::{Damage, DamageDealer, DamageModifier, Damageable}, id::Id};

#[derive(Clone)]
pub struct DamageInstance {
	amount: f32,
	modifiers: BdlsPtr<[BdlsPtr<dyn DamageModifier>]>,
	target: BdlsPtr<BdlsMutex<dyn Damageable>>,
	damage_dealer: Option<BdlsPtr<BdlsMutex<dyn DamageDealer>>>,
}

impl DamageInstance {
	pub fn from(
		damage: &Damage,
		target: impl Damageable + 'static,
		damage_dealer: Option<impl DamageDealer + 'static>,
	) -> Self {
		Self {
			amount: damage.amount,
			modifiers: damage.modifiers.clone(),
			target: BdlsPtr::new(BdlsMutex::new(target)),
			damage_dealer: damage_dealer.map(|dd| BdlsPtr::new(BdlsMutex::new(dd)) as BdlsPtr<BdlsMutex<dyn DamageDealer>>)
		}
	}

	pub fn inflict(self) {
		let modifiers = self.modifiers.clone();
		let shared_target = self.target.clone();
		let shared_dealer_opt = self.damage_dealer.clone();

		let shared_damage = BdlsPtr::new(BdlsMutex::new(self));

		for modifier in modifiers.iter() {
			modifier.apply(shared_damage.clone());
			modifier.add_effects(shared_damage.clone());
		}

		let Ok(damage) = shared_damage.try_lock() else {return};
		let Ok(mut target) = shared_target.try_lock() else {return};

		target.damage(&damage);

		if let Some(dealer_shared) = shared_dealer_opt
		&& let Ok(mut dealer) = dealer_shared.try_lock() {
			dealer.award_damage(&damage, &*target);
		}
	}

	#[must_use]
	pub const fn amount(&self) -> f32 {
		self.amount
	}

	pub fn scale(
		&mut self,
		resistance_attribute: &Id,
		strength_attribute: &Id,
		allow_negative: bool,
	) {
		let Ok(target_ref) = self.target.try_lock() else {return};
		let dealer_ref = self.damage_dealer.as_ref().and_then(|some| some.try_lock().ok());

		self.amount = scale_damage(
			self.amount(),
			resistance_attribute,
			strength_attribute,
			&*target_ref,
			dealer_ref.as_deref(),
			allow_negative
		);
	}

	pub fn modifiers(& self) -> Iter<'_, BdlsPtr<dyn DamageModifier>> {
		self.modifiers.iter()
	}

	#[must_use]
	pub fn target(&self) -> BdlsPtr<BdlsMutex<dyn Damageable>> {
		self.target.clone()
	}

	#[must_use]
	pub fn damage_dealer(&self) -> Option<BdlsPtr<BdlsMutex<dyn DamageDealer>>> {
		self.damage_dealer.clone()
	}
}

unsafe impl Sync for DamageInstance {}