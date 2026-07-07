#[cfg(feature = "thread-safe")]
mod aliases {
	use std::sync::{Arc, Mutex, MutexGuard};

	pub type BdlsMutex<T> = Mutex<T>;
	pub type BdlsPtr<T> = Arc<T>;
	pub type BdlsGuard<'a, T> = MutexGuard<'a, T>;
}

#[cfg(not(feature = "thread-safe"))]
mod aliases {
	use std::cell::{RefCell, RefMut};
	use std::rc::Rc;

	pub type BdlsMutex<T> = RefCell<T>;
	pub type BdlsPtr<T> = Rc<T>;
	pub type BdlsGuard<'a, T> = RefMut<'a, T>;
}

#[cfg(feature = "thread-safe")]
#[macro_use]
mod macros {
	macro_rules! bdls_err_type {
		($lifetime:lifetime,$content:ty) => {
			std::sync::PoisonError<BdlsGuard<$lifetime, $content>>
		};
	}
}

#[cfg(not(feature = "thread-safe"))]
#[macro_use]
mod macros {
	macro_rules! bdls_err_type {
		($lifetime:lifetime,$content:ty) => {
			std::cell::BorrowMutError
		};
	}
}

pub use aliases::*;

pub trait Lockable<T: ?Sized> {
	#[allow(clippy::missing_errors_doc)]
	fn try_lock(&self) -> Result<BdlsGuard<'_, T>, bdls_err_type!('_, T)>;
}

impl<T: ?Sized> Lockable<T> for BdlsMutex<T> {
	#[inline]
	fn try_lock(&self) -> Result<BdlsGuard<'_, T>, bdls_err_type!('_, T)> {
		#[cfg(feature = "thread-safe")]
		{
			self.lock()
		}
		#[cfg(not(feature = "thread-safe"))]
		{
			self.try_borrow_mut()
		}
	}
}