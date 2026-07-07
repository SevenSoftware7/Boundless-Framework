use crate::{math::lerp};

pub trait AttributeModifier {
	#[must_use]
	fn apply_to(&self, base_value: f32) -> f32;

	#[must_use]
	fn operator(&self) -> AttributeModifierOperator { AttributeModifierOperator::Set }

	#[inline]
	#[must_use]
	fn strength(&self) -> f32 { 1.0 }
}

#[derive(Clone, Copy, PartialEq, Eq, Debug)]
pub enum AttributeModifierOperator {
	Set,
	Multiply { stacking: bool },
	Add,
	MoreThan { deferred: bool },
	LessThan { deferred: bool }
}

#[derive(Clone, Copy, PartialEq, Debug)]
pub enum AttributeModifierOperation {
	Set { to: f32 },
	Multiply { with: f32, stacking: bool },
	Add { value: f32 },
	MoreThan { minimum: f32, deferred: bool },
	LessThan { maximum: f32, deferred: bool }
}

impl AttributeModifier for AttributeModifierOperation {
	fn apply_to(&self, base_value: f32) -> f32 {
		match self {
			Self::Set { to } => *to,
			Self::Multiply { with, .. } => base_value * with,
			Self::Add { value } => base_value + *value,
			Self::MoreThan { minimum, .. } => base_value.max(*minimum),
			Self::LessThan { maximum, .. } => base_value.min(*maximum)
		}
	}

	fn operator(&self) -> AttributeModifierOperator {
		match self {
			Self::Set { to: _ } => AttributeModifierOperator::Set,
			Self::Multiply { with: _, stacking } => AttributeModifierOperator::Multiply { stacking: *stacking },
			Self::Add { value: _ } => AttributeModifierOperator::Add,
			Self::MoreThan { minimum: _, deferred } => AttributeModifierOperator::MoreThan { deferred: *deferred },
			Self::LessThan { maximum: _, deferred } => AttributeModifierOperator::LessThan { deferred: *deferred }
		}
	}
}

pub fn apply_modifiers<'a, I, T>(modifiers: I, base_value: f32) -> f32
where
	I: Iterator<Item = T>,
	T: AttributeModifier + 'a,
{
	let mut init = base_value;
	let mut max = f32::INFINITY;
	let mut min = f32::NEG_INFINITY;
	let mut add = 0.0;
	let mut mult = 1.0;

	for entry in modifiers {
		match entry.operator() {
			// NON-DEFERRED = calculation starts HIGHER/LOWER THAN value
			AttributeModifierOperator::MoreThan { deferred: false } | AttributeModifierOperator::LessThan { deferred: false } => {
				init = lerp(init, entry.apply_to(init), entry.strength());
			},
			// DEFERRED MoreThan = calculation result CANNOT GO LOWER than value
			AttributeModifierOperator::MoreThan { deferred: true } => {
				let strength = entry.strength();
				let res = entry.apply_to(min);
				if min.is_finite() {
					min = lerp(min, res, strength);
				} else if strength > 0.0 {
					min = res;
				}
			},
			// DEFERRED LessThan = calculation result CANNOT GO HIGHER than value
			AttributeModifierOperator::LessThan { deferred: true } => {
				let strength = entry.strength();
				let res = entry.apply_to(max);
				if max.is_finite() {
					max = lerp(max, res, strength);
				} else if strength > 0.0 {
					max = res;
				}
			},
			// Stacking Multiply STACKS with other Multiplies
			//
			// 0.5 + 0.5 = 0.25
			// 1.2 + 1.2 + 1.44
			AttributeModifierOperator::Multiply { stacking: true } => {
				mult = lerp(mult, entry.apply_to(mult), entry.strength());
			},
			// Add the 'SUM' of the operation, operations DO NOT STACK
			//
			// 0.5 + 0.5 = 0.0
			// 1.2 + 1.2 + 1.4
			_ => {
				add += lerp(base_value, entry.apply_to(base_value), entry.strength()) - base_value;
			},
		}
	}

	((init + add) * mult).clamp(min, max)
}


pub trait TraitModifierIterator {
	fn apply_to(&mut self, base_value: f32) -> f32;
}

impl<I, T> TraitModifierIterator for I
where
	I: Iterator<Item = T>,
	T: AttributeModifier,
{
	fn apply_to(&mut self, base_value: f32) -> f32 {
		apply_modifiers(self, base_value)
	}
}