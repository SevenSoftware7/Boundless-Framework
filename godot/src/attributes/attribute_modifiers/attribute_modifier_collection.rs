use boundless::id::Id;

use godot::meta::{Element, ToArg};
use godot::meta::conv::ByValue;
use godot::meta::shape::{ClassHeritage, GodotElementShape, GodotShape};
use godot::prelude::*;

use crate::{AttributeModifierEntry, AttributeModifierEntryWrapper};


#[derive(GodotClass, Default, Clone)]
#[class(init, tool)]
pub struct AttributeModifierCollection {
	array: Array<Option<Gd<AttributeModifierEntry>>>
}

#[godot_api]
impl AttributeModifierCollection {
	#[must_use]
	pub fn shape() -> GodotShape {
		GodotShape::TypedArray {
			element: GodotElementShape::Class {
				class_id: AttributeModifierEntry::class_id(),
				heritage: ClassHeritage::Resource
			}
		}
	}

	pub const fn from(array: Array<Option<Gd<AttributeModifierEntry>>>) -> Self {
		// modifiers_by_attribute: array.iter_shared()
		// 	.flatten()
		// 	.into_group_map_by(|e| e.bind().id.as_id().clone()),
		Self { array }
	}

	#[func]
	pub fn clear(&mut self) {
		self.array.clear();
	}

	pub fn iter_attrs(&self, id: &Id) -> impl Iterator<Item = AttributeModifierEntryWrapper> {
		self.array.iter_shared()
			.flatten()
			.filter(move |e| *e.bind().id.as_id() == *id)
			.map(AttributeModifierEntryWrapper::wrap)
	}
}


impl GodotConvert for AttributeModifierCollection {
	type Via = Array<Option<Gd<AttributeModifierEntry>>>;

	fn godot_shape() -> GodotShape {
		Self::shape()
	}
}

impl FromGodot for AttributeModifierCollection {
	fn try_from_godot(via: Self::Via) -> Result<Self, godot::prelude::ConvertError> {
		Ok(Self::from(via))
	}
}

impl ToGodot for AttributeModifierCollection {
	type Pass = ByValue;

	fn to_godot(&self) -> ToArg<'_, Self::Via, Self::Pass> {
		self.array.clone()
	}
}

impl Var for AttributeModifierCollection {
	type PubType = Self::Via;

	fn var_get(field: &Self) -> Self::Via {
		Self::to_godot(field)
	}

	fn var_set(field: &mut Self, value: Self::Via) {
		*field = Self::from_godot(value);
	}

	fn var_pub_get(field: &Self) -> Self::PubType {
		Self::to_godot(field)
	}

	fn var_pub_set(field: &mut Self, value: Self::PubType) {
		*field = Self::from_godot(value);
	}
}

impl Export for AttributeModifierCollection {}

impl Element for AttributeModifierCollection {}