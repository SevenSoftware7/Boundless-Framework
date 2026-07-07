use std::cell::RefCell;
use std::collections::HashMap;
use std::sync::atomic::{AtomicI64, Ordering};

use godot::builtin::{AnyDictionary, Callable, Variant, Vector3, Vector4};
use godot::classes::{Engine, IObject, IRefCounted, Object, RefCounted, notify::ObjectNotification};
use godot::global::godot_error;
use godot::obj::{Base, Gd, NewGd, Singleton};
use godot::prelude::{godot_api, GodotClass};

#[derive(Clone, Copy)]
pub struct WaterDisplacementQuery {
	pub x: f32,
	pub z: f32,
	pub intensity: f32,
	pub scale: f32,
}

pub struct WaterDisplacementFetchBatch {
	rust_ids: Vec<u64>,
	handle_ids: Vec<i64>,
	queries: Vec<WaterDisplacementQuery>,
}

impl WaterDisplacementFetchBatch {
	#[must_use]
	pub const fn len(&self) -> usize {
		self.queries.len()
	}

	#[must_use]
	pub const fn is_empty(&self) -> bool {
		self.queries.is_empty()
	}

	#[must_use]
	pub fn queries(&self) -> &[WaterDisplacementQuery] {
		&self.queries
	}
}

struct RustSubscriber {
	query: Box<dyn FnMut() -> Option<WaterDisplacementQuery>>,
	update: Box<dyn FnMut(Vector3)>,
}

#[derive(Clone)]
struct HandleSubscriber {
	id: i64,
	query_callable: Option<Callable>,
	info_callable: Callable,
	update_callable: Option<Callable>,
}

thread_local! {
	static RUST_SUBSCRIBERS: RefCell<HashMap<u64, RustSubscriber>> = RefCell::new(HashMap::new());
	static HANDLE_SUBSCRIBERS: RefCell<HashMap<i64, HandleSubscriber>> = RefCell::new(HashMap::new());
}

static NEXT_HANDLE_ID: AtomicI64 = AtomicI64::new(1);

fn next_handle_id() -> i64 {
	NEXT_HANDLE_ID.fetch_add(1, Ordering::Relaxed)
}

fn compute_displacement(query: WaterDisplacementQuery, time_seconds: f32) -> Vector3 {
	let scale = query.scale.max(0.0001);
	let phase_x = query.x.mul_add(scale, time_seconds * 0.2);
	let phase_z = query.z.mul_add(scale, - (time_seconds * 0.2));

	let lateral = 0.05 * query.intensity;
	let vertical = query.intensity;

	Vector3::new(
		phase_x.sin() * lateral,
		(phase_x * 1.7 + phase_z * 1.3).sin() * vertical,
		phase_z.cos() * lateral,
	)
}

fn variant_to_query(value: &Variant) -> Option<WaterDisplacementQuery> {
	if let Ok(v4) = value.try_to::<Vector4>() {
		return Some(WaterDisplacementQuery {
			x: v4.x,
			z: v4.y,
			intensity: v4.z,
			scale: v4.w,
		});
	}

	if let Ok(dict) = value.try_to::<AnyDictionary>() {
		let position_key = Variant::from("position");
		let x_key = Variant::from("x");
		let z_key = Variant::from("z");
		let intensity_key = Variant::from("intensity");
		let scale_key = Variant::from("scale");

		let position = dict
			.get(&position_key)
			.and_then(|value| value.try_to::<Vector3>().ok());
		let x = dict
			.get(&x_key)
			.and_then(|value| value.try_to::<f32>().ok());
		let z = dict
			.get(&z_key)
			.and_then(|value| value.try_to::<f32>().ok());

		let intensity = dict
			.get(&intensity_key)
			.and_then(|value| value.try_to::<f32>().ok())
			.unwrap_or(1.0);
		let scale = dict
			.get(&scale_key)
			.and_then(|value| value.try_to::<f32>().ok())
			.unwrap_or(1.0);

		let (final_x, final_z) = if let Some(pos) = position {
			(pos.x, pos.z)
		} else {
			(x?, z?)
		};

		return Some(WaterDisplacementQuery {
			x: final_x,
			z: final_z,
			intensity,
			scale,
		});
	}

	None
}


#[derive(GodotClass)]
#[class(base = RefCounted)]
pub struct WaterDisplacementHandle {
	#[base]
	base: Base<RefCounted>,
	id: i64,
}

#[godot_api]
impl IRefCounted for WaterDisplacementHandle {
	fn init(base: Base<RefCounted>) -> Self {
		Self { base, id: 0 }
	}

	fn on_notification(&mut self, what: ObjectNotification) {
		if what == ObjectNotification::PREDELETE
			&& self.id > 0 {
				let _ = WaterDisplacement::remove_handle_internal(self.id);
				self.id = 0;
			}
	}
}

#[godot_api]
impl WaterDisplacementHandle {
	#[must_use]
	pub const fn get_id(&self) -> i64 {
		self.id
	}
	#[allow(clippy::missing_const_for_fn)]
	#[must_use]
	#[func(rename=get_id)]
	pub fn gd_get_id(&self) -> i64 {
		self.get_id()
	}

	#[func]
	pub fn connect_query(&mut self, query_callable: Callable) -> bool {
		if self.id <= 0 {
			return false;
		}
		WaterDisplacement::set_query_callable_internal(self.id, query_callable)
	}

	#[func]
	pub fn connect_update(&mut self, update_callable: Callable) -> bool {
		if self.id <= 0 {
			return false;
		}
		WaterDisplacement::set_update_callable_internal(self.id, update_callable)
	}

	#[func]
	pub fn kill(&mut self) -> bool {
		if self.id <= 0 {
			return false;
		}

		let removed = WaterDisplacement::remove_handle_internal(self.id);
		if removed {
			self.id = 0;
		}
		removed
	}
}

#[derive(GodotClass)]
#[class(base = Object, singleton)]
pub struct WaterDisplacement {
	#[base]
	base: Base<Object>,
}

#[godot_api]
impl IObject for WaterDisplacement {
	fn init(base: Base<Object>) -> Self {
		Self { base }
	}
}

impl WaterDisplacement {
	pub fn subscribe_rust(
		query: impl FnMut() -> Option<WaterDisplacementQuery> + 'static,
		update: impl FnMut(Vector3) + 'static,
	) -> u64 {
		let id = next_handle_id().cast_unsigned();
		RUST_SUBSCRIBERS.with(|subscribers| {
			subscribers.borrow_mut().insert(
				id,
				RustSubscriber {
					query: Box::new(query),
					update: Box::new(update),
				},
			);
		});
		id
	}

	#[must_use]
	pub fn unsubscribe_rust(id: u64) -> bool {
		RUST_SUBSCRIBERS.with(|subscribers| subscribers.borrow_mut().remove(&id).is_some())
	}

	pub fn clear_rust_subscribers() {
		RUST_SUBSCRIBERS.with(|subscribers| subscribers.borrow_mut().clear());
	}

	pub fn dispatch_updates(time_seconds: f32) {
		let engine = Engine::singleton();
		let Some(mut singleton) = engine.get_singleton("WaterDisplacement") else {
			return;
		};

		let _ = singleton.call("emit_updates_internal", &[Variant::from(time_seconds)]);
	}

	#[must_use]
	pub fn collect_fetch_queries() -> WaterDisplacementFetchBatch {
		let mut batch = WaterDisplacementFetchBatch {
			rust_ids: Vec::new(),
			handle_ids: Vec::new(),
			queries: Vec::new(),
		};

		RUST_SUBSCRIBERS.with(|subscribers| {
			let mut subscribers = subscribers.borrow_mut();
			for (id, subscriber) in subscribers.iter_mut() {
				if let Some(query) = (subscriber.query)() {
					batch.rust_ids.push(*id);
					batch.queries.push(query);
				}
			}
		});

		HANDLE_SUBSCRIBERS.with(|subscribers| {
			let mut subscribers = subscribers.borrow_mut();
			for subscriber in subscribers.values_mut() {
				if subscriber.update_callable.is_none() {
					continue;
				}

				let info = if let Some(query_callable) = &subscriber.query_callable {
					query_callable.call(&[])
				} else {
					subscriber.info_callable.call(&[])
				};

				let Some(query) = variant_to_query(&info) else {
					continue;
				};

				batch.handle_ids.push(subscriber.id);
				batch.queries.push(query);
			}
		});

		batch
	}

	pub fn dispatch_fetched_updates(batch: &WaterDisplacementFetchBatch, fetched_xyz: &[f32]) {
		let total = batch.len();
		if fetched_xyz.len() < total * 3 {
			godot_error!(
				"WaterDisplacement fetch output too small: got {}, expected {}",
				fetched_xyz.len(),
				total * 3
			);
			return;
		}

		for (i, rust_id) in batch.rust_ids.iter().enumerate() {
			let offset = i * 3;
			let displacement = Vector3::new(
				fetched_xyz[offset],
				fetched_xyz[offset + 1],
				fetched_xyz[offset + 2],
			);

			RUST_SUBSCRIBERS.with(|subscribers| {
				if let Some(subscriber) = subscribers.borrow_mut().get_mut(rust_id) {
					(subscriber.update)(displacement);
				}
			});
		}

		let handle_base = batch.rust_ids.len();
		for (i, handle_id) in batch.handle_ids.iter().enumerate() {
			let output_index = handle_base + i;
			let offset = output_index * 3;
			let displacement = Vector3::new(
				fetched_xyz[offset],
				fetched_xyz[offset + 1],
				fetched_xyz[offset + 2],
			);

			HANDLE_SUBSCRIBERS.with(|subscribers| {
				if let Some(subscriber) = subscribers.borrow_mut().get_mut(handle_id)
					&& let Some(update_callable) = &subscriber.update_callable {
						update_callable.call_deferred(&[Variant::from(displacement)]);
					}
			});
		}
	}

	fn create_handle_internal(info_callable: Callable) -> i64 {
		let id = next_handle_id();
		HANDLE_SUBSCRIBERS.with(|subscribers| {
			subscribers.borrow_mut().insert(
				id,
				HandleSubscriber {
					id,
					query_callable: None,
					info_callable,
					update_callable: None,
				},
			);
		});
		id
	}

	fn remove_handle_internal(id: i64) -> bool {
		HANDLE_SUBSCRIBERS.with(|subscribers| subscribers.borrow_mut().remove(&id).is_some())
	}

	fn set_query_callable_internal(id: i64, query_callable: Callable) -> bool {
		let mut updated = false;
		HANDLE_SUBSCRIBERS.with(|subscribers| {
			if let Some(subscriber) = subscribers.borrow_mut().get_mut(&id) {
				subscriber.query_callable = Some(query_callable);
				updated = true;
			}
		});
		updated
	}

	fn set_update_callable_internal(id: i64, update_callable: Callable) -> bool {
		let mut updated = false;
		HANDLE_SUBSCRIBERS.with(|subscribers| {
			if let Some(subscriber) = subscribers.borrow_mut().get_mut(&id) {
				subscriber.update_callable = Some(update_callable);
				updated = true;
			}
		});
		updated
	}

	fn clear_handle_subscribers_internal() {
		HANDLE_SUBSCRIBERS.with(|subscribers| subscribers.borrow_mut().clear());
	}
}

#[godot_api]
impl WaterDisplacement {
	#[func]
	pub fn create_handle(&mut self, info_callable: Callable) -> Gd<WaterDisplacementHandle> {
		let id = Self::create_handle_internal(info_callable);
		let mut handle = WaterDisplacementHandle::new_gd();
		handle.bind_mut().id = id;
		handle
	}

	#[func]
	pub fn remove_handle(&mut self, handle_id: i64) -> bool {
		if handle_id <= 0 {
			return false;
		}
		Self::remove_handle_internal(handle_id)
	}

	#[func]
	pub fn clear_handles(&mut self) {
		Self::clear_handle_subscribers_internal();
	}

	#[func]
	pub fn emit_updates_internal(&mut self, time_seconds: f32) {
		let batch = Self::collect_fetch_queries();
		if batch.is_empty() {
			return;
		}

		let mut fetched = Vec::with_capacity(batch.len() * 3);
		for query in batch.queries() {
			let displacement = compute_displacement(*query, time_seconds);
			fetched.push(displacement.x);
			fetched.push(displacement.y);
			fetched.push(displacement.z);
		}

		Self::dispatch_fetched_updates(&batch, &fetched);
	}
}