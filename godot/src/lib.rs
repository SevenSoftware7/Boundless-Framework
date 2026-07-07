#![allow(clippy::must_use_candidate)] // must_use has false_positives with #[export]-annotated fields

pub mod id;
pub mod entity;
pub mod attributes;
pub mod damage;
pub mod rendering;
pub mod water;

pub use id::*;
pub use entity::*;
pub use attributes::*;
pub use damage::*;
pub use rendering::*;
pub use water::*;


#[cfg(feature = "standalone")]
mod entry {
	use godot::prelude::*;

	struct BoundlessExtension;

	#[gdextension]
	unsafe impl ExtensionLibrary for BoundlessExtension {}
}