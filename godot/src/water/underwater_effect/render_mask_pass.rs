use godot::builtin::{Array, Color, PackedColorArray, PackedByteArray, Projection, Rid};
use godot::classes::rendering_device::{DrawFlags, UniformType};
use godot::classes::{RdUniform, RenderingDevice};
use godot::obj::{Gd, NewGd};

use crate::water::underwater_effect::projection_utils;

#[allow(clippy::too_many_arguments)]
#[inline]
pub(crate) fn run_render_mask_pass(
	rd: &mut Gd<RenderingDevice>,
	water_framebuffer: Rid,
	render_pipeline: Rid,
	render_shader: Rid,
	displacement_sampler: Rid,
	displacement_texture: Rid,
	info_buffer: Rid,
	vertex_array: Rid,
	index_array: Rid,
	world_to_clip: Projection,
) -> bool {
	let mut render_uniforms_set0 = Array::<Gd<RdUniform>>::new();
	let mut displacement_uniform = RdUniform::new_gd();
	displacement_uniform.set_uniform_type(UniformType::SAMPLER_WITH_TEXTURE);
	displacement_uniform.set_binding(0);
	displacement_uniform.add_id(displacement_sampler);
	displacement_uniform.add_id(displacement_texture);
	render_uniforms_set0.push(&displacement_uniform);
	let render_set0 = rd.uniform_set_create(&render_uniforms_set0, render_shader, 0);

	let mut render_uniforms_set1 = Array::<Gd<RdUniform>>::new();
	let mut info_uniform = RdUniform::new_gd();
	info_uniform.set_uniform_type(UniformType::STORAGE_BUFFER);
	info_uniform.set_binding(0);
	info_uniform.add_id(info_buffer);
	render_uniforms_set1.push(&info_uniform);
	let render_set1 = rd.uniform_set_create(&render_uniforms_set1, render_shader, 1);

	if !render_set0.is_valid() || !render_set1.is_valid() {
		if render_set0.is_valid() {
			rd.free_rid(render_set0);
		}
		if render_set1.is_valid() {
			rd.free_rid(render_set1);
		}
		return false;
	}

	let render_push: PackedByteArray = projection_utils::pack_projection(world_to_clip);

	let mut clear_colors = PackedColorArray::new();
	clear_colors.push(Color::from_rgba(0.0, 0.0, 0.0, 0.0));

	rd.draw_command_begin_label("Render Water Mask", Color::from_rgb(1.0, 1.0, 1.0));
	let draw_list = rd
		.draw_list_begin_ex(water_framebuffer)
		.draw_flags(DrawFlags::CLEAR_ALL)
		.clear_color_values(&clear_colors)
		.clear_depth_value(0.0)
		.done();
	rd.draw_list_bind_render_pipeline(draw_list, render_pipeline);
	rd.draw_list_bind_vertex_array(draw_list, vertex_array);
	rd.draw_list_bind_index_array(draw_list, index_array);
	rd.draw_list_bind_uniform_set(draw_list, render_set0, 0);
	rd.draw_list_bind_uniform_set(draw_list, render_set1, 1);
	#[allow(clippy::cast_possible_truncation)]
	rd.draw_list_set_push_constant(draw_list, &render_push, render_push.len() as u32);
	rd.draw_list_draw(draw_list, true, 2);
	rd.draw_list_end();
	rd.draw_command_end_label();

	rd.free_rid(render_set0);
	rd.free_rid(render_set1);
	true
}
