use godot::prelude::*;

use boundless::{damage::DamageInstance, sync::{BdlsMutex, BdlsPtr}};
use godot::register::{GodotClass, godot_api};

use crate::GodotId;

#[derive(GodotClass, Clone)]
#[class(base=RefCounted, no_init, rename=DamageInstance)]
pub struct GodotDamageInstance {
	damage_instance: BdlsPtr<BdlsMutex<DamageInstance>>,
}

#[godot_api]
impl GodotDamageInstance {
	pub const fn from(damage_instance: BdlsPtr<BdlsMutex<DamageInstance>>) -> Self {
		Self {
			damage_instance,
		}
	}
	pub fn gd_from(damage_instance: BdlsPtr<BdlsMutex<DamageInstance>>) -> Gd<Self> {
		Gd::from_object(Self::from(damage_instance))
	}

	#[func]
	#[must_use]
	pub fn get_amount(&self) -> f32 {
		self.damage_instance.borrow().amount()
	}

	#[func]
	pub fn scale_damage(
		&mut self,
		resistance_attribute: GodotId,
		strength_attribute: GodotId,
		#[opt(default=false)] allow_negative: bool,
	) {
		self.damage_instance.borrow_mut()
			.scale(
				&resistance_attribute.into(),
				&strength_attribute.into(),
				allow_negative
			);
	}
}