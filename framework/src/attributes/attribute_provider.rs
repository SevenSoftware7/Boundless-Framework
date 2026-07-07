use std::{collections::HashMap, hash::BuildHasher};

use crate::id::Id;

pub trait AttributeProvider {
	fn get_attribute(&self, id: &Id) -> Option<f32>;
}

impl<S: BuildHasher> AttributeProvider for HashMap<Id, f32, S> {
	fn get_attribute(&self, id: &Id) -> Option<f32> {
		self.get(id).copied()
	}
}