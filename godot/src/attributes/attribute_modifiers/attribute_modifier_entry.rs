use boundless::attributes::{AttributeModifier, AttributeModifierOperator};
use godot::{obj::{Gd, OnEditor}, prelude::GodotClass};

use crate::{GodotAttributeModifier, GodotId};


#[derive(GodotClass, Debug)]
#[class(base=Resource, init, tool)]
pub struct AttributeModifierEntry {
	#[export]
	pub id: GodotId,
	#[export]
	pub modifier: OnEditor<Gd<GodotAttributeModifier>>,
	#[export(range = (0.0, 1.0, 0.005))]
	#[init(val=1.0)]
	pub strength: f32,
}

impl AttributeModifierEntry {
	pub fn from(id: impl Into<GodotId>, modifier: Gd<GodotAttributeModifier>, strength: Option<f32>) -> Self {
		let mut sentinel: OnEditor<Gd<GodotAttributeModifier>> = OnEditor::default();
		sentinel.init(modifier);

		Self {
			id: id.into(),
			modifier: sentinel,
			strength: strength.unwrap_or(1.0)
		}
	}
}

impl AttributeModifier for AttributeModifierEntry {
	fn apply_to(&self, base_value: f32) -> f32 {
		self.modifier.bind().apply_to(base_value)
	}
	fn operator(&self) -> AttributeModifierOperator {
		self.modifier.bind().operator()
	}
	fn strength(&self) -> f32 {
		self.strength
	}
}

impl Clone for AttributeModifierEntry {
	fn clone(&self) -> Self {
		Self::from(
			self.id.clone(),
			self.modifier.clone(),
			Some(self.strength)
		)
	}
}


pub struct AttributeModifierEntryWrapper(Gd<AttributeModifierEntry>);

impl AttributeModifierEntryWrapper {
	pub const fn wrap(item: Gd<AttributeModifierEntry>) -> Self {
		Self(item)
	}
}

impl AttributeModifier for AttributeModifierEntryWrapper {
	fn apply_to(&self, base_value: f32) -> f32 {
		self.0.bind().apply_to(base_value)
	}
	fn operator(&self) -> AttributeModifierOperator {
		self.0.bind().operator()
	}
	fn strength(&self) -> f32 {
		self.0.bind().strength
	}
}

impl From<Gd<AttributeModifierEntry>> for AttributeModifierEntryWrapper {
	fn from(value: Gd<AttributeModifierEntry>) -> Self {
		Self::wrap(value)
	}
}