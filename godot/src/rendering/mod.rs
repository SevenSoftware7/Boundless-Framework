use godot::{classes::{RdShaderSource, RenderingDevice, rendering_device::{ShaderLanguage, ShaderStage}}, obj::{Gd, NewGd}};
use godot::builtin::{Rid, PackedByteArray, Vector2i};

#[must_use]
pub fn groups_for_size(size: Vector2i, group_size: u32) -> (u32, u32) {
	let width = size.x.max(1).cast_unsigned();
	let height = size.y.max(1).cast_unsigned();
	let x_groups = (width - 1) / group_size + 1;
	let y_groups = (height - 1) / group_size + 1;
	(x_groups, y_groups)
}

#[must_use]
pub fn packed_bytes_from_i32(values: &[i32]) -> PackedByteArray {
	let mut bytes = Vec::with_capacity(std::mem::size_of_val(values));
	for value in values {
		bytes.extend_from_slice(&value.to_ne_bytes());
	}
	PackedByteArray::from(bytes.as_slice())
}

#[must_use]
pub fn sanitize_glsl_shader_source(source: &str) -> String {
	source
		.lines()
		.filter(|line| !line.trim_start().starts_with("#["))
		.collect::<Vec<_>>()
		.join("\n")
}

pub fn create_shader_from_source(
	rd: &mut Gd<RenderingDevice>,
	source: &str,
	stage: ShaderStage,
	language: ShaderLanguage,
) -> Rid {
	let mut shader_source = RdShaderSource::new_gd();
	shader_source.set_stage_source(stage, source);
	shader_source.set_language(language);

	let Some(spirv) = rd.shader_compile_spirv_from_source(&shader_source) else {
		return Rid::Invalid;
	};

	rd.shader_create_from_spirv(&spirv)
}
