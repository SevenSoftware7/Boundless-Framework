use core::fmt;

use string_cache::DefaultAtom;

pub(crate) fn normalize_id_name(name: &str) -> String {
	name.to_lowercase().replace(' ', "_")
}

#[derive(Clone, Default, PartialEq, Eq, Hash, Debug)]
pub struct Id {
	id: DefaultAtom,
}

impl Id {
	pub fn from_unnormalized(name: impl AsRef<str>) -> Self {
		let normalized = normalize_id_name(name.as_ref());
		Self {
			id: DefaultAtom::from(normalized),
		}
	}

	/// # Panics
	///
	/// Will panic if given string `normalized_name` is not normalized.
	/// When in doubt, use `from_unnormalized()`
	#[must_use]
	pub fn from_normalized(normalized_name: impl AsRef<str>) -> Self {
		let str = normalized_name.as_ref();
		debug_assert_eq!(str, normalize_id_name(str), "Id name must be normalized (lowercase, no spaces)");
		Self {
			id: DefaultAtom::from(str),
		}
	}

	#[must_use]
	pub fn id(&self) -> &str {
		&self.id
	}
}


impl From<&str> for Id {
	fn from(value: &str) -> Self {
		Self::from_unnormalized(value)
	}
}

impl From<String> for Id {
	fn from(value: String) -> Self {
		Self::from_unnormalized(value)
	}
}

impl fmt::Display for Id {
	fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
		let str = &*self.id;
		write!(f, "{str}")
	}
}