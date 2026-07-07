use boundless::attributes::{AttributeModifier, AttributeModifierOperator};
use godot::{classes::IResource, obj::{Base, WithBaseField}, prelude::*, register::info::{PropertyInfo, PropertyUsageFlags}};

use crate::AttributeOperator;

#[derive(GodotClass)]
#[class(base=Resource, init, tool, rename=AttributeModifier)]
pub struct GodotAttributeModifier {
	#[var(set = set_operation)]
	#[export]
	#[init(val=AttributeOperator::Multiply)]
	pub operation: AttributeOperator,
	#[export]
	#[init(val=1.0)]
	pub value: f32,
	#[export]
	pub is_stacking: bool,
	#[export]
	pub is_deferred: bool,

	#[base]
	base: Base<Resource>,
}

#[godot_api]
impl GodotAttributeModifier {
	pub const IS_STACKING_PROPERTY: &'static str = "is_stacking";
	pub const IS_DEFERRED_PROPERTY: &'static str = "is_deferred";
	pub const OPERATION_PROPERTY: &'static str = "operation";

	#[func]
	fn set_operation(&mut self, operation: AttributeOperator) {
		self.operation = operation;
		self.base().signals().property_list_changed().emit();
	}
}

impl AttributeModifier for GodotAttributeModifier {
	fn apply_to(&self, base_value: f32) -> f32 {
		match self.operation {
			AttributeOperator::Set => self.value,
			AttributeOperator::Multiply => self.value * base_value,
			AttributeOperator::Add => base_value + self.value,
			AttributeOperator::MoreThan => base_value.max(self.value),
			AttributeOperator::LessThan => base_value.min(self.value),
		}
	}

	fn operator(&self) -> AttributeModifierOperator {
		match self.operation {
			AttributeOperator::Set => AttributeModifierOperator::Set,
			AttributeOperator::Multiply => AttributeModifierOperator::Multiply { stacking: self.is_stacking },
			AttributeOperator::Add => AttributeModifierOperator::Add,
			AttributeOperator::MoreThan => AttributeModifierOperator::MoreThan { deferred: self.is_deferred },
			AttributeOperator::LessThan => AttributeModifierOperator::LessThan { deferred: self.is_deferred },
		}
	}
}

#[godot_api]
impl IResource for GodotAttributeModifier {
	// This is broken in gdext for now, see https://github.com/godot-rust/gdext/issues/1427

	fn on_validate_property(&self, property: &mut PropertyInfo) {
		match property.property_name.to_string().as_str() {
			Self::IS_STACKING_PROPERTY => {
				property.usage = match self.operation {
					AttributeOperator::Multiply => PropertyUsageFlags::DEFAULT,
					_ => PropertyUsageFlags::NONE,
				}
			},
			Self::IS_DEFERRED_PROPERTY => {
				property.usage = match self.operation {
					AttributeOperator::MoreThan | AttributeOperator::LessThan => PropertyUsageFlags::DEFAULT,
					_ => PropertyUsageFlags::NONE,
				};
			},
			_ => {},
		}
	}
}

pub struct GodotAttributeModifierWrapper(Gd<GodotAttributeModifier>);

impl GodotAttributeModifierWrapper {
	pub const fn wrap(item: Gd<GodotAttributeModifier>) -> Self {
		Self(item)
	}
}

impl AttributeModifier for GodotAttributeModifierWrapper {
	fn apply_to(&self, base_value: f32) -> f32 {
		self.0.bind().apply_to(base_value)
	}
	fn operator(&self) -> AttributeModifierOperator {
		self.0.bind().operator()
	}
	fn strength(&self) -> f32 {
		self.0.bind().strength()
	}
}

impl From<Gd<GodotAttributeModifier>> for GodotAttributeModifierWrapper {
	fn from(value: Gd<GodotAttributeModifier>) -> Self {
		Self::wrap(value)
	}
}