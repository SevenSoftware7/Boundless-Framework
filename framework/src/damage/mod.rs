pub mod damage_instance;
pub mod damageable;
pub mod damage_dealer;
pub mod damage_dealer_proxy;
pub mod damage_modifier;

pub use damage_instance::*;
pub use damageable::*;
pub use damage_dealer::*;
pub use damage_dealer_proxy::*;
pub use damage_modifier::*;

use crate::sync::BdlsPtr;

#[derive(Default)]
pub struct Damage {
	pub amount: f32,
	pub modifiers: BdlsPtr<[BdlsPtr<dyn DamageModifier>]>,
}

impl Damage {
	#[inline]
	pub fn new(
		amount: f32,
		modifiers: impl IntoIterator<Item = BdlsPtr<dyn DamageModifier>>,
	) -> Self {
		Self {
			amount,
			modifiers: modifiers.into_iter().collect(),
		}
	}

	#[inline]
	pub fn instantiate(
		&self,
		target: impl Damageable + 'static,
		damage_dealer: Option<impl DamageDealer + 'static>,
	) -> DamageInstance {
		DamageInstance::from(self, target, damage_dealer)
	}

	#[inline]
	pub fn inflict<T: Damageable + 'static, D: DamageDealer + 'static>(
		&self,
		target: T,
		damage_dealer: Option<D>,
	) {
		let instance = self.instantiate(target, damage_dealer);
		instance.inflict();
	}
}