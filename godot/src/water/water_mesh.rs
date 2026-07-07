use godot::{
	builtin::{Color, StringName, Variant},
	classes::{IMeshInstance3D, Mesh, MeshInstance3D, notify::Node3DNotification},
	meta::ToGodot, obj::{Base, Gd, InstanceId, WithBaseField},
	prelude::{GodotClass, godot_api},
};

use crate::WaterMeshRegistry;


#[derive(GodotClass)]
#[class(base = MeshInstance3D, init, tool)]
pub struct WaterMesh {
	#[base]
	base: Base<MeshInstance3D>,
	tracked_mesh_id: Option<InstanceId>,

	#[var(set = set_shallow_color)]
	#[export(color_no_alpha)]
	#[init(val = Color::from_rgb(0.1395, 0.279_225, 0.45))]
	pub shallow_color: Color,

	#[var(set = set_deep_color)]
	#[export(color_no_alpha)]
	#[init(val = Color::from_rgb(0.068, 0.127, 0.2))]
	pub deep_color: Color,

	#[var(set = set_water_intensity)]
	#[export]
	#[init(val = 1.85)]
	pub water_intensity: f32,

	#[var(set = set_water_scale)]
	#[export]
	#[init(val = 45.0)]
	pub water_scale: f32,

	#[var(set = set_fog_distance)]
	#[export]
	#[init(val = 60.0)]
	pub fog_distance: f32,

	#[var(set = set_fog_fade)]
	#[export(range = (0.0, 20.0, 0.1))]
	#[init(val = 3.5)]
	pub fog_fade: f32,

	#[var(set = set_transparency_distance)]
	#[export]
	#[init(val = 100.0)]
	pub transparency_distance: f32,

	#[var(set = set_transparency_fade)]
	#[export(range = (0.0, 20.0, 0.1))]
	#[init(val = 2.5)]
	pub transparency_fade: f32,
}

#[godot_api]
impl IMeshInstance3D for WaterMesh {
	fn on_notification(&mut self, what: Node3DNotification) {
		if what == Node3DNotification::EXTENSION_RELOADED {
			WaterMeshRegistry::add(self.base().instance_id());
		}
	}

	fn enter_tree(&mut self) {
		self.connect_mesh_callbacks();
		WaterMeshRegistry::add(self.base().instance_id());
	}

	fn exit_tree(&mut self) {
		self.disconnect_mesh_callbacks();
		WaterMeshRegistry::remove(self.base().instance_id());
	}

	fn on_set(&mut self, property: StringName, value: Variant) -> bool {
		if property != "mesh" {
			return false;
		}

		let Ok(mesh) = value.try_to::<Option<Gd<Mesh>>>() else {
			return false;
		};

		self.set_base_mesh(mesh.as_ref());
		true
	}
}

#[godot_api]
impl WaterMesh {
	fn connect_mesh_callbacks(&mut self) -> bool {
		let current_mesh_id = self.base().get_mesh().map(|mesh| mesh.instance_id());
		if self.tracked_mesh_id == current_mesh_id {
			return false;
		}

		self.disconnect_mesh_callbacks();

		if let Some(mut mesh) = self.base().get_mesh() {
			let rebuild_callable = WaterMeshRegistry::schedule_rebuild_callable(self.base().instance_id());
			if !mesh.is_connected("changed", &rebuild_callable) {
				mesh.connect("changed", &rebuild_callable);
			}
			// mesh.signals().changed().connect(WaterMeshRegistry::schedule_rebuild);

			// let update_params_callable = Callable::from_object_method(&self.to_gd(), "update_all_shader_parameters");
			// if !mesh.is_connected("changed", &update_params_callable) {
			// 	mesh.connect("changed", &update_params_callable);
			// }
			// mesh.signals().changed().connect(update_params_callable);

			self.tracked_mesh_id = Some(mesh.instance_id());
		}

		true
	}

	fn disconnect_mesh_callbacks(&mut self) -> bool {
		let Some(mesh_id) = self.tracked_mesh_id else {
			return false;
		};

		if let Ok(mut mesh) = Gd::<Mesh>::try_from_instance_id(mesh_id) {
			let rebuild_callable = WaterMeshRegistry::schedule_rebuild_callable(self.base().instance_id());
			if mesh.is_connected("changed", &rebuild_callable) {
				mesh.disconnect("changed", &rebuild_callable);
			}
			// mesh.signals().changed().disconnect(WaterMeshRegistry::schedule_rebuild);

			// let update_params_callable = Callable::from_object_method(&self.to_gd(), "update_all_shader_parameters");
			// if mesh.is_connected("changed", &update_params_callable) {
			// 	mesh.disconnect("changed", &update_params_callable);
			// }
			// mesh.signals().changed().disconnect(WaterMeshRegistry::schedule_rebuild);
		}

		self.tracked_mesh_id = None;

		true
	}

	fn set_base_mesh(&mut self, mesh: Option<&Gd<Mesh>>) {
		match mesh.as_ref() {
			Some(mesh_ref) => self.base_mut().set_mesh(*mesh_ref),
			None => self.base_mut().set_mesh(Option::<&Gd<Mesh>>::None),
		}

		if self.connect_mesh_callbacks() {
			WaterMeshRegistry::schedule_rebuild(self.base().instance_id());
			self.update_all_shader_parameters();
		}
	}


	fn update_shallow_color(&self) {
		if let Some(mesh) = self.base().get_mesh()
			&& let Some(mut material) = mesh.surface_get_material(0) {
				material.set("shader_parameter/water_shallow_color", &self.shallow_color.to_variant());
			}
	}
	#[func]
	pub fn set_shallow_color(&mut self, color: Color) {
		self.shallow_color = color;
		self.update_shallow_color();
	}

	fn update_deep_color(&self) {
		if let Some(mesh) = self.base().get_mesh()
			&& let Some(mut material) = mesh.surface_get_material(0) {
				material.set("shader_parameter/water_deep_color", &self.deep_color.to_variant());
			}
	}
	#[func]
	pub fn set_deep_color(&mut self, color: Color) {
		self.deep_color = color;
		self.update_deep_color();
	}

	fn update_water_intensity(&self) {
		if let Some(mesh) = self.base().get_mesh()
			&& let Some(mut material) = mesh.surface_get_material(0) {
				material.set("shader_parameter/water_intensity", &self.water_intensity.to_variant());
			}
	}
	#[func]
	pub fn set_water_intensity(&mut self, intensity: f32) {
		self.water_intensity = intensity;
		self.update_water_intensity();
	}

	fn update_water_scale(&self) {
		if let Some(mesh) = self.base().get_mesh()
			&& let Some(mut material) = mesh.surface_get_material(0) {
				material.set("shader_parameter/water_scale", &self.water_scale.to_variant());
			}
	}
	#[func]
	pub fn set_water_scale(&mut self, scale: f32) {
		self.water_scale = scale;
		self.update_water_scale();
	}

	fn update_fog_distance(&self) {
		if let Some(mesh) = self.base().get_mesh()
			&& let Some(mut material) = mesh.surface_get_material(0) {
				material.set("shader_parameter/water_fog_distance", &self.fog_distance.to_variant());
			}
	}
	#[func]
	pub fn set_fog_distance(&mut self, distance: f32) {
		self.fog_distance = distance;
		self.update_fog_distance();
	}

	fn update_fog_fade(&self) {
		if let Some(mesh) = self.base().get_mesh()
			&& let Some(mut material) = mesh.surface_get_material(0) {
				material.set("shader_parameter/water_fog_fade", &self.fog_fade.to_variant());
			}
	}
	#[func]
	pub fn set_fog_fade(&mut self, fade: f32) {
		self.fog_fade = fade;
		self.update_fog_fade();
	}

	fn update_transparency_distance(&self) {
		if let Some(mesh) = self.base().get_mesh()
			&& let Some(mut material) = mesh.surface_get_material(0) {
				material.set("shader_parameter/water_transparency_distance", &self.transparency_distance.to_variant());
			}
	}
	#[func]
	pub fn set_transparency_distance(&mut self, distance: f32) {
		self.transparency_distance = distance;
		self.update_transparency_distance();
	}

	fn update_transparency_fade(&self) {
		if let Some(mesh) = self.base().get_mesh()
			&& let Some(mut material) = mesh.surface_get_material(0) {
				material.set("shader_parameter/water_transparency_fade", &self.transparency_fade.to_variant());
			}
	}
	#[func]
	pub fn set_transparency_fade(&mut self, fade: f32) {
		self.transparency_fade = fade;
		self.update_transparency_fade();
	}

	fn update_all_shader_parameters(&self) {
		self.update_shallow_color();
		self.update_deep_color();
		self.update_water_intensity();
		self.update_water_scale();
		self.update_fog_distance();
		self.update_fog_fade();
		self.update_transparency_distance();
		self.update_transparency_fade();
	}

	#[must_use]
	pub fn get_water_mesh(&self) -> Option<Gd<Mesh>> {
		self.base().get_mesh()
	}
}