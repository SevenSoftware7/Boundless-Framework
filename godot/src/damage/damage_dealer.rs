use boundless::{attributes::AttributeProvider, damage::{DamageDealer, DamageInstance, Damageable}, id::Id};
use godot::prelude::*;

#[derive(Clone)]
pub struct GodotDamageDealer(DynGd<Node, dyn DamageDealer>);

impl GodotDamageDealer {
	#[must_use]
	pub fn from(damage_dealer: DynGd<Node, dyn DamageDealer>) -> Self {
		Self(damage_dealer)
	}
}

impl AttributeProvider for GodotDamageDealer {
	fn get_attribute(&self, id: &Id) -> Option<f32> {
		self.0.dyn_bind().get_attribute(id)
	}
}

impl DamageDealer for GodotDamageDealer {
	fn award_damage(&mut self, damage: &DamageInstance, target: &dyn Damageable) {
		self.0.dyn_bind_mut().award_damage(damage, target);
	}
}