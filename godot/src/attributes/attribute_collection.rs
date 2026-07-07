use std::collections::{HashMap, hash_map};

use godot::{meta::{Element, ToArg, conv::ByValue, shape::{GodotElementShape, GodotShape}}, prelude::*};

use boundless::{attributes::AttributeProvider, id::Id};

use crate::GodotId;


#[derive(GodotClass, Default, Clone, PartialEq, Debug)]
#[class(init, tool)]
pub struct AttributeCollection {
	attribute_values: HashMap<GodotId, f32>,
}

#[godot_api]
impl AttributeCollection {
	pub const SHAPE: GodotShape = GodotShape::TypedDictionary {
		key: GodotId::ELEMENT_SHAPE,
		value: GodotElementShape::Builtin {
			variant_type: VariantType::FLOAT
		},
	};

	pub fn set(&mut self, id: GodotId, value: f32) -> Option<f32> {
		self.attribute_values.insert(id, value)
	}
	#[func(rename=set)]
	#[allow(clippy::needless_pass_by_value)]
	pub fn gd_set(&mut self, id: GodotId, value: f32) -> f32 {
		self.set(id, value)
			.unwrap_or_default()
	}

	#[must_use]
	pub fn get(&self, id: &GodotId) -> Option<&f32> {
		self.attribute_values
			.get(id)
	}
	#[func(rename=get)]
	#[must_use]
	#[allow(clippy::needless_pass_by_value)]
	pub fn gd_get(&self, id: GodotId, default_value: f32) -> f32 {
		self.get(&id)
			.copied()
			.unwrap_or(default_value)
	}

	#[func]
	#[must_use]
	#[allow(clippy::needless_pass_by_value)]
	pub fn remove(&mut self, id: GodotId) -> bool {
		self.attribute_values
			.remove(&id)
			.is_some()
	}

	#[func]
	pub fn clear(&mut self) {
		self.attribute_values.clear();
	}

	#[func]
	#[must_use]
	#[allow(clippy::needless_pass_by_value)]
	pub fn contains_attribute(&self, id: GodotId) -> bool {
		self.attribute_values
			.contains_key(&id)
	}

	#[must_use]
	pub fn iter(&self) -> hash_map::Iter<'_, GodotId, f32> {
		<&Self as IntoIterator>::into_iter(self)
	}
	#[must_use]
	pub fn iter_mut(&mut self) -> hash_map::IterMut<'_, GodotId, f32> {
		<&mut Self as IntoIterator>::into_iter(self)
	}
}


impl IntoIterator for AttributeCollection {
	type Item = (GodotId, f32);
	type IntoIter = hash_map::IntoIter<GodotId, f32>;

	fn into_iter(self) -> Self::IntoIter {
		self.attribute_values.into_iter()
	}
}

impl<'a> IntoIterator for &'a AttributeCollection {
	type Item = (&'a GodotId, &'a f32);
	type IntoIter = hash_map::Iter<'a, GodotId, f32>;

	fn into_iter(self) -> Self::IntoIter {
		self.attribute_values.iter()
	}
}

impl<'a> IntoIterator for &'a mut AttributeCollection {
	type Item = (&'a GodotId, &'a mut f32);
	type IntoIter = hash_map::IterMut<'a, GodotId, f32>;

	fn into_iter(self) -> Self::IntoIter {
		self.attribute_values.iter_mut()
	}
}

impl FromIterator<(GodotId, f32)> for AttributeCollection {
	fn from_iter<T: IntoIterator<Item = (GodotId, f32)>>(iter: T) -> Self {
		let attribute_values = iter.into_iter().collect();
		Self { attribute_values }
	}
}

impl Extend<(GodotId, f32)> for AttributeCollection {
	fn extend<T: IntoIterator<Item = (GodotId, f32)>>(&mut self, iter: T) {
		self.attribute_values.extend(iter);
	}
}

impl AttributeProvider for AttributeCollection {
	fn get_attribute(&self, id: &Id) -> Option<f32> {
		self.get(&id.clone().into())
			.copied()
	}
}

impl GodotConvert for AttributeCollection {
	type Via = Dictionary<GodotId, f32>;

	fn godot_shape() -> GodotShape {
		Self::SHAPE
	}
}

impl FromGodot for AttributeCollection {
	fn try_from_godot(via: Self::Via) -> Result<Self, godot::prelude::ConvertError> {
		Ok(
			Self::from_iter(&via)
		)
	}
}

impl ToGodot for AttributeCollection {
	type Pass = ByValue;

	fn to_godot(&self) -> ToArg<'_, Self::Via, Self::Pass> {
		self.into_iter()
			.fold(Dictionary::new(), |mut dict, (id, value)| {
				let _ = dict.insert(id.clone(), *value);
				dict
			})
	}
}

impl Var for AttributeCollection {
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

impl Export for AttributeCollection {}

impl Element for AttributeCollection {}