pub mod damageable;
pub mod damage_dealer;
pub mod damage_instance;
pub mod damage_modifier;
pub mod area;

pub use damageable::*;
pub use damage_dealer::*;
pub use damage_instance::*;
pub use damage_modifier::*;
pub use area::*;

use godot::{builtin::Array, classes::Resource, obj::{DynGd, Gd}, prelude::*};

use boundless::{damage::{Damage, DamageDealer, DamageModifier, Damageable}, sync::{BdlsMutex, BdlsPtr}};
use itertools::Itertools;

#[derive(GodotClass)]
#[class(base=Resource, init, tool, rename = Damage)]
pub struct GodotDamage {
	#[export]
	#[init(val=1.0)]
	pub amount: f32,
	#[export]
	pub modifiers: Array<Option<DynGd<Resource, dyn DamageModifier>>>,
}

#[godot_api]
impl GodotDamage {
	pub fn build(&self) -> Damage {
		Damage::new(
			self.amount,
			self.modifiers.iter_shared()
				.flatten()
				.unique()
				.map(|modifier| BdlsPtr::<DamageModifierWrapper>::new(modifier.into()) as BdlsPtr<dyn DamageModifier>)
		)
	}

	#[func]
	pub fn instantiate(
		&mut self,
		target: DynGd<Node, dyn Damageable>,
		damage_dealer: Option<DynGd<Node, dyn DamageDealer>>,
	) -> Gd<GodotDamageInstance> {
		let damage = self.build();

		let damage_instance = damage.instantiate(
			GodotDamageable::from(target),
			damage_dealer.map(GodotDamageDealer::from)
		);

		GodotDamageInstance::gd_from(BdlsPtr::new(BdlsMutex::new(damage_instance)))
	}

	#[func]
	pub fn inflict(
		&mut self,
		target: DynGd<Node, dyn Damageable>,
		damage_dealer: Option<DynGd<Node, dyn DamageDealer>>,
	) {
		let damage = self.build();

		let damage_instance = damage.instantiate(
			GodotDamageable::from(target),
			damage_dealer.map(GodotDamageDealer::from)
		);
		damage_instance.inflict();
	}
}

impl From<GodotDamage> for Damage {
	fn from(val: GodotDamage) -> Self {
		val.build()
	}
}