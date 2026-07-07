use godot::builtin::{
	Array, PackedByteArray, Projection, Rid,
};
use godot::classes::rendering_device::IndexBufferFormat;
use godot::classes::RenderingDevice;
use godot::obj::{Gd, Singleton, WithBaseField};

use crate::water::{WaterMesh, WaterMeshRegistry};

pub(crate) struct WaterMeshGpuData {
	pub vertex_floats: Vec<f32>,
	pub indices: Vec<u32>,
	pub water_info_floats: Vec<f32>,
	pub water_params_floats: Vec<f32>,
}

pub(crate) fn collect_water_mesh_data() -> Option<WaterMeshGpuData> {
	let mut vertex_floats = Vec::<f32>::new();
	let mut indices = Vec::<u32>::new();
	let mut water_info_floats = Vec::<f32>::new();
	let mut water_params_floats = Vec::<f32>::new();

	let mesh_reg = WaterMeshRegistry::singleton();

	let mesh_ids = mesh_reg.bind().get_mesh_ids();
	let mut mesh_index: u32 = 0;
	for mesh_id in mesh_ids {
		let Ok(mesh_obj) = Gd::<WaterMesh>::try_from_instance_id(mesh_id) else {
			continue;
		};

		let mesh = mesh_obj.bind();
		if !mesh.base().is_visible_in_tree() {
			continue;
		}

		let mesh_reg_bind = mesh_reg.bind();
		let Some((water_vertices, water_indices)) = mesh_reg_bind.get_mesh_data(&mesh_id) else {
			continue;
		};

		#[allow(clippy::cast_possible_truncation)]
		let idx_offset = vertex_floats.len() as u32 / 4;
		for vertex in water_vertices.as_slice().iter().copied() {
			vertex_floats.push(vertex.x);
			vertex_floats.push(vertex.y);
			vertex_floats.push(vertex.z);
			vertex_floats.push(f32::from_bits(mesh_index));
		}

		for index in water_indices.as_slice().iter().copied() {
			indices.push(index.cast_unsigned() + idx_offset);
		}

		let transform = Projection::from(mesh.base().get_global_transform());
		append_projection_floats(&mut water_info_floats, transform);
		water_info_floats.push(mesh.water_intensity);
		water_info_floats.push(mesh.water_scale);
		water_info_floats.push(0.0);
		water_info_floats.push(0.0);

		let shallow = mesh.shallow_color.srgb_to_linear();
		let deep = mesh.deep_color.srgb_to_linear();
		water_params_floats.push(shallow.r);
		water_params_floats.push(shallow.g);
		water_params_floats.push(shallow.b);
		water_params_floats.push(mesh.fog_distance);
		water_params_floats.push(deep.r);
		water_params_floats.push(deep.g);
		water_params_floats.push(deep.b);
		water_params_floats.push(mesh.fog_fade);
		water_params_floats.push(mesh.transparency_distance);
		water_params_floats.push(mesh.transparency_fade);
		water_params_floats.push(0.0);
		water_params_floats.push(0.0);

		mesh_index += 1;
	}

	if vertex_floats.is_empty() || indices.is_empty() || water_info_floats.is_empty() {
		return None;
	}

	Some(WaterMeshGpuData {
		vertex_floats,
		indices,
		water_info_floats,
		water_params_floats,
	})
}

pub(crate) fn create_storage_buffer(rd: &mut Gd<RenderingDevice>, floats: &[f32]) -> Rid {
	let mut bytes = Vec::<u8>::with_capacity(floats.len() * 4);
	for value in floats {
		bytes.extend_from_slice(&value.to_ne_bytes());
	}
	let packed = PackedByteArray::from(bytes.as_slice());
	#[allow(clippy::cast_possible_truncation)]
	rd.storage_buffer_create_ex(packed.len() as u32)
		.data(&packed)
		.done()
}

pub(crate) fn create_vertex_buffers(
	rd: &mut Gd<RenderingDevice>,
	vertex_floats: &[f32],
	vertex_format: i64,
	vertex_count: u32,
) -> (Rid, Rid) {
	let mut bytes = Vec::<u8>::with_capacity(vertex_floats.len() * 4);
	for value in vertex_floats {
		bytes.extend_from_slice(&value.to_ne_bytes());
	}
	let packed = PackedByteArray::from(bytes.as_slice());
	#[allow(clippy::cast_possible_truncation)]
	let vertex_buffer = rd
		.vertex_buffer_create_ex(packed.len() as u32)
		.data(&packed)
		.done();

	if !vertex_buffer.is_valid() {
		return (Rid::Invalid, Rid::Invalid);
	}

	let mut src_buffers = Array::<Rid>::new();
	src_buffers.push(vertex_buffer);
	let vertex_array = rd.vertex_array_create(vertex_count, vertex_format, &src_buffers);
	(vertex_buffer, vertex_array)
}

pub(crate) fn create_index_buffers(rd: &mut Gd<RenderingDevice>, indices: &[u32]) -> (Rid, Rid) {
	let mut bytes = Vec::<u8>::with_capacity(indices.len() * 4);
	for index in indices {
		bytes.extend_from_slice(&index.to_ne_bytes());
	}
	let packed = PackedByteArray::from(bytes.as_slice());
	#[allow(clippy::cast_possible_truncation)]
	let index_buffer = rd
		.index_buffer_create_ex(indices.len() as u32, IndexBufferFormat::UINT32)
		.data(&packed)
		.done();
	if !index_buffer.is_valid() {
		return (Rid::Invalid, Rid::Invalid);
	}

	#[allow(clippy::cast_possible_truncation)]
	let index_array = rd.index_array_create(index_buffer, 0, indices.len() as u32);
	(index_buffer, index_array)
}

fn append_projection_floats(output: &mut Vec<f32>, projection: Projection) {
	for col in projection.cols {
		output.push(col.x);
		output.push(col.y);
		output.push(col.z);
		output.push(col.w);
	}
}
