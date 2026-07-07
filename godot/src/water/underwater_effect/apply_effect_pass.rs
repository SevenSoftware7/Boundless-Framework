use godot::builtin::{Array, Color, PackedByteArray, Projection, Rid};
use godot::classes::rendering_device::UniformType;
use godot::classes::{RdUniform, RenderingDevice};
use godot::obj::{Gd, NewGd};

use crate::water::underwater_effect::projection_utils;

#[allow(clippy::too_many_arguments)]
#[inline]
pub(crate) fn run_apply_underwater_pass(
	rd: &mut Gd<RenderingDevice>,
	compute_pipeline: Rid,
	compute_shader: Rid,
	depth_sampler: Rid,
	color: Rid,
	depth: Rid,
	water_map: Rid,
	parameters_buffer: Rid,
	projection: Projection,
	screen_size: (i32, i32),
	groups: (u32, u32),
) {
	let mut uniforms_set0 = Array::<Gd<RdUniform>>::new();
	let mut color_uniform = RdUniform::new_gd();
	color_uniform.set_uniform_type(UniformType::IMAGE);
	color_uniform.set_binding(0);
	color_uniform.add_id(color);
	uniforms_set0.push(&color_uniform);
	let compute_set0 = rd.uniform_set_create(&uniforms_set0, compute_shader, 0);

	let mut uniforms_set1 = Array::<Gd<RdUniform>>::new();
	let mut depth_uniform = RdUniform::new_gd();
	depth_uniform.set_uniform_type(UniformType::SAMPLER_WITH_TEXTURE);
	depth_uniform.set_binding(0);
	depth_uniform.add_id(depth_sampler);
	depth_uniform.add_id(depth);
	uniforms_set1.push(&depth_uniform);
	let compute_set1 = rd.uniform_set_create(&uniforms_set1, compute_shader, 1);

	let mut uniforms_set2 = Array::<Gd<RdUniform>>::new();
	let mut water_map_uniform = RdUniform::new_gd();
	water_map_uniform.set_uniform_type(UniformType::IMAGE);
	water_map_uniform.set_binding(0);
	water_map_uniform.add_id(water_map);
	uniforms_set2.push(&water_map_uniform);
	let compute_set2 = rd.uniform_set_create(&uniforms_set2, compute_shader, 2);

	let mut uniforms_set3 = Array::<Gd<RdUniform>>::new();
	let mut parameters_uniform = RdUniform::new_gd();
	parameters_uniform.set_uniform_type(UniformType::STORAGE_BUFFER);
	parameters_uniform.set_binding(0);
	parameters_uniform.add_id(parameters_buffer);
	uniforms_set3.push(&parameters_uniform);
	let compute_set3 = rd.uniform_set_create(&uniforms_set3, compute_shader, 3);

	if !compute_set0.is_valid()
		|| !compute_set1.is_valid()
		|| !compute_set2.is_valid()
		|| !compute_set3.is_valid()
	{
		if compute_set0.is_valid() {
			rd.free_rid(compute_set0);
		}
		if compute_set1.is_valid() {
			rd.free_rid(compute_set1);
		}
		if compute_set2.is_valid() {
			rd.free_rid(compute_set2);
		}
		if compute_set3.is_valid() {
			rd.free_rid(compute_set3);
		}
		return;
	}

	let mut compute_push_values = Vec::<u8>::with_capacity(18 * 4 + 8);
	projection_utils::append_projection_bytes(&mut compute_push_values, projection.inverse());
	compute_push_values.extend_from_slice(&projection.z_near().to_ne_bytes());
	compute_push_values.extend_from_slice(&projection.z_far().to_ne_bytes());
	compute_push_values.extend_from_slice(&screen_size.0.to_ne_bytes());
	compute_push_values.extend_from_slice(&screen_size.1.to_ne_bytes());
	let compute_push = PackedByteArray::from(compute_push_values.as_slice());

	rd.draw_command_begin_label("Render Underwater Effect", Color::from_rgb(1.0, 1.0, 1.0));
	let compute_list = rd.compute_list_begin();
	rd.compute_list_bind_compute_pipeline(compute_list, compute_pipeline);
	rd.compute_list_bind_uniform_set(compute_list, compute_set0, 0);
	rd.compute_list_bind_uniform_set(compute_list, compute_set1, 1);
	rd.compute_list_bind_uniform_set(compute_list, compute_set2, 2);
	rd.compute_list_bind_uniform_set(compute_list, compute_set3, 3);
	#[allow(clippy::cast_possible_truncation)]
	rd.compute_list_set_push_constant(compute_list, &compute_push, compute_push.len() as u32);
	rd.compute_list_dispatch(compute_list, groups.0, groups.1, 1);
	rd.compute_list_end();
	rd.draw_command_end_label();

	rd.free_rid(compute_set0);
	rd.free_rid(compute_set1);
	rd.free_rid(compute_set2);
	rd.free_rid(compute_set3);
}
