use std::collections::{HashMap, HashSet};

use godot::builtin::{PackedInt32Array, PackedVector3Array, Callable};
use godot::classes::mesh::ArrayType;
use godot::classes::RenderingServer;
use godot::meta::ToGodot;
use godot::obj::{Gd, IndexEnum, InstanceId, Singleton};
use godot::register::{godot_api, GodotClass};

use crate::water::WaterMesh;

#[derive(GodotClass)]
#[class(singleton, init)]
pub struct WaterMeshRegistry {
	water_mesh_ids: HashSet<InstanceId>,
	water_data: HashMap<InstanceId, (PackedVector3Array, PackedInt32Array)>,
	scheduled_rebuilds: HashSet<InstanceId>,
}

#[godot_api]
impl WaterMeshRegistry {
	pub fn add_id(&mut self, id: InstanceId) {
		// godot_print!("Adding water mesh with ID {} to registry.", id);
		if self.water_mesh_ids.insert(id) {
			self.schedule_rebuild_buffers(id);
			// godot_print!("Added water mesh with ID {} to registry and scheduled buffers rebuild.", id);
		}
	}

	pub fn remove_id(&mut self, id: &InstanceId) {
		// godot_print!("Removing water mesh with ID {} from registry.", id);
		if self.water_mesh_ids.remove(id) {
			self.clear_buffers(id);
			// godot_print!("Removed water mesh with ID {} from registry and cleared its buffers.", id);
		}
	}

	pub fn schedule_rebuild_buffers(&mut self, id: InstanceId) {
		let already_scheduled = ! self.scheduled_rebuilds.is_empty();

		self.scheduled_rebuilds.insert(id);
		if already_scheduled {
			// godot_print!("Rebuild already scheduled, skipping redundant schedule call.");
			return;
		}

		// godot_print!("Scheduling water buffers rebuild from schedule_rebuild_buffers.");

		let rebuild_callable = Self::scheduled_rebuild_callable();
		rebuild_callable.call_deferred(&[]);
	}

	fn scheduled_buffers_rebuild(&mut self) {
		// godot_print!("Running scheduled water buffers rebuild for {} meshes.", self.scheduled_rebuilds.len());

		let scheduled_ids = self.scheduled_rebuilds.drain().collect::<Vec<_>>();
		for id in scheduled_ids {
			self.rebuild_buffers(id);
		}

		self.scheduled_rebuilds.clear();
	}

	fn rebuild_buffers(&mut self, instance_id: InstanceId) -> bool {
		let server = RenderingServer::singleton();

		let Some(mesh_object) = Gd::<WaterMesh>::try_from_instance_id(instance_id).ok() else {
			return false;
		};

		let mesh = mesh_object.bind();
		let Some(surface_mesh) = mesh.get_water_mesh() else {
			return false;
		};

		let (water_vertices, water_indices) = self.water_data
			.entry(instance_id)
			.and_modify(|e| {
				e.0.clear();
				e.1.clear();
			})
			.or_insert_with(|| (PackedVector3Array::new(), PackedInt32Array::new()));

		let surface_count = surface_mesh.get_surface_count();
		for surface_index in 0..surface_count {
			let surface_arrays = server.mesh_surface_get_arrays(surface_mesh.get_rid(), surface_index);

			let Some(vertex_variant) = surface_arrays.get(ArrayType::VERTEX.to_index()) else {
				continue;
			};

			let Ok(surface_vertices) = vertex_variant.try_to::<PackedVector3Array>() else {
				continue;
			};

			let surface_indices = surface_arrays
				.get(ArrayType::INDEX.to_index())
				.and_then(|value| value.try_to::<PackedInt32Array>().ok())
				.unwrap_or_default();

			// let vertex_offset = (vertex_floats.len() / 4) as u32;
			for vertex in surface_vertices.as_slice().iter().copied() {
				water_vertices.push(vertex);
			}

			for index in surface_indices.as_slice().iter().copied() {
				water_indices.push(index/*  + vertex_offset */);
			}
		}

		true
	}

	pub fn clear_buffers(&mut self, instance_id: &InstanceId) {
		self.water_data.remove(instance_id);
	}


	#[func]
	pub fn schedule_rebuild(id: InstanceId) {
		let mut registry = Self::singleton();
		registry.bind_mut().schedule_rebuild_buffers(id);
	}
	#[must_use]
	pub fn schedule_rebuild_callable(instance_id: InstanceId) -> Callable {
		Callable::from_class_static("WaterMeshRegistry", "schedule_rebuild").bind(&[instance_id.to_variant()])
	}

	#[func]
	fn scheduled_rebuild() {
		let mut registry = Self::singleton();
		registry.bind_mut().scheduled_buffers_rebuild();
	}
	#[must_use]
	pub fn scheduled_rebuild_callable() -> Callable {
		Callable::from_class_static("WaterMeshRegistry", "scheduled_rebuild")
	}

	#[func]
	pub fn clear(id: InstanceId) {
		let mut registry = Self::singleton();
		registry.bind_mut().clear_buffers(&id);
	}
	#[must_use]
	pub fn clear_callable(instance_id: InstanceId) -> Callable {
		Callable::from_class_static("WaterMeshRegistry", "clear").bind(&[instance_id.to_variant()])
	}

	#[func]
	pub fn add(id: InstanceId) {
		let mut registry = Self::singleton();
		registry.bind_mut().add_id(id);
	}
	#[must_use]
	pub fn add_callable(instance_id: InstanceId) -> Callable {
		Callable::from_class_static("WaterMeshRegistry", "add").bind(&[instance_id.to_variant()])
	}

	#[func]
	pub fn remove(id: InstanceId) {
		let mut registry = Self::singleton();
		registry.bind_mut().remove_id(&id);
	}
	#[must_use]
	pub fn remove_callable(instance_id: InstanceId) -> Callable {
		Callable::from_class_static("WaterMeshRegistry", "remove").bind(&[instance_id.to_variant()])
	}

	#[must_use]
	pub fn get_mesh_ids(&self) -> Vec<InstanceId> {
		let mut ids = self.water_mesh_ids.iter().copied().collect::<Vec<_>>();
		ids.sort_by_key(|id| id.to_i64());
		ids
	}
	#[must_use]
	pub fn get_mesh_data(&self, instance_id: &InstanceId) -> Option<&(PackedVector3Array, PackedInt32Array)> {
		self.water_data.get(instance_id)
	}
}