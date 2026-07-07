use std::vec;

use godot::prelude::*;

use boundless::{attributes::{AttributeProvider, TraitModifierIterator}, id::Id};

use crate::{AttributeModifierCollection, attribute_collection::AttributeCollection};

#[derive(GodotClass, Clone)]
#[class(base=Resource, init, tool, rename=AttributeProvider)]
pub struct GodotAttributeProvider {
	#[export]
	pub attributes: AttributeCollection,
	#[export]
	pub modifiers: AttributeModifierCollection,
}

impl AttributeProvider for GodotAttributeProvider {
	fn get_attribute(&self, id: &Id) -> Option<f32> {
		self.attributes.get_attribute(id)
			.map(|base_value| self.modifiers
			.iter_attrs(id)
			.apply_to(base_value))
	}
}