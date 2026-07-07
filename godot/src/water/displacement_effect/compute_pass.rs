use godot::builtin::{Array, Color, PackedByteArray, Rid, Vector2i};
use godot::classes::rendering_device::UniformType;
use godot::classes::{RdUniform, RenderingDevice};
use godot::obj::{Gd, NewGd};

use crate::rendering::groups_for_size;

pub fn run_displacement_compute_pass(
	rd: &mut Gd<RenderingDevice>,
	compute_pipeline: Rid,
	compute_shader: Rid,
	water_displacement_map: Rid,
	render_size: Vector2i,
	time_seconds: f32,
) {
	let (x_groups, y_groups) = groups_for_size(render_size, 8);

	let mut uniforms = Array::<Gd<RdUniform>>::new();
	let mut image_uniform = RdUniform::new_gd();
	image_uniform.set_uniform_type(UniformType::IMAGE);
	image_uniform.set_binding(0);
	image_uniform.add_id(water_displacement_map);
	uniforms.push(&image_uniform);

	let uniform_set = rd.uniform_set_create(&uniforms, compute_shader, 0);
	if !uniform_set.is_valid() {
		return;
	}

	let mut push_values = Vec::with_capacity(4 + 8);
	push_values.extend_from_slice(&render_size.x.to_ne_bytes());
	push_values.extend_from_slice(&render_size.y.to_ne_bytes());
	push_values.extend_from_slice(&time_seconds.to_ne_bytes());
	push_values.extend_from_slice(&0.2f32.to_ne_bytes());
	let push = PackedByteArray::from(push_values.as_slice());

	rd.draw_command_begin_label("Render Water Displacement", Color::from_rgb(1.0, 1.0, 1.0));
	let compute_list = rd.compute_list_begin();
	rd.compute_list_bind_compute_pipeline(compute_list, compute_pipeline);
	rd.compute_list_bind_uniform_set(compute_list, uniform_set, 0);
	#[allow(clippy::cast_possible_truncation)]
	rd.compute_list_set_push_constant(compute_list, &push, push.len() as u32);
	rd.compute_list_dispatch(compute_list, x_groups, y_groups, 1);
	rd.compute_list_end();
	rd.draw_command_end_label();

	rd.free_rid(uniform_set);
}
