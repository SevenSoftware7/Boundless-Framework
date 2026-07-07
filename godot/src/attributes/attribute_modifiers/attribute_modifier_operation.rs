use std::fmt::Display;

use godot::prelude::*;

#[derive(GodotConvert, Var, Export, Default, Clone, Eq, PartialEq, Debug, Hash)]
#[godot(via = GString)]
pub enum AttributeOperator {
	Set,
	#[default]
	Multiply,
	Add,
	MoreThan,
	LessThan,
}

impl Display for AttributeOperator {
	fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
		match self {
			Self::Set => write!(f, "Set"),
			Self::Multiply => write!(f, "Multiply"),
			Self::Add => write!(f, "Add"),
			Self::MoreThan => write!(f, "More Than"),
			Self::LessThan => write!(f, "Less Than"),
		}
	}
}