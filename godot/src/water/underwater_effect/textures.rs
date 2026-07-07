use godot::builtin::{StringName, Vector2i};
use godot::classes::rendering_device::{DataFormat, TextureSamples, TextureUsageBits};
use godot::classes::RenderSceneBuffersRd;
use godot::obj::{EngineBitfield, Gd};

pub(crate) const UNDERWATER_CONTEXT: &str = "UnderwaterEffect";
pub(crate) const WATER_MAP_NAME: &str = "water_map";
pub(crate) const WATER_DEPTH_NAME: &str = "water_depth";

pub(crate) fn ensure_water_map_textures(
	scene_buffers: &mut Gd<RenderSceneBuffersRd>,
	view_count: u32,
	render_size: Vector2i,
) {
	let context = StringName::from(UNDERWATER_CONTEXT);
	let map_name = StringName::from(WATER_MAP_NAME);
	let depth_name = StringName::from(WATER_DEPTH_NAME);

	if scene_buffers.has_texture(&context, &map_name)
		&& let Some(texture_format) = scene_buffers.get_texture_format(&context, &map_name)
			&& (texture_format.get_width() != render_size.x.cast_unsigned()
				|| texture_format.get_height() != render_size.y.cast_unsigned())
			{
				scene_buffers.clear_context(&context);
			}

	if !scene_buffers.has_texture(&context, &map_name) {
		#[allow(clippy::cast_possible_truncation)]
		scene_buffers.create_texture(
			&context,
			&map_name,
			DataFormat::R32G32B32A32_SFLOAT,
			(TextureUsageBits::COLOR_ATTACHMENT_BIT | TextureUsageBits::STORAGE_BIT).ord() as u32,
			TextureSamples::SAMPLES_1,
			render_size,
			view_count,
			1,
			true,
			false,
		);
	}

	if !scene_buffers.has_texture(&context, &depth_name) {
		#[allow(clippy::cast_possible_truncation)]
		scene_buffers.create_texture(
			&context,
			&depth_name,
			DataFormat::D32_SFLOAT,
			TextureUsageBits::DEPTH_STENCIL_ATTACHMENT_BIT.ord() as u32,
			TextureSamples::SAMPLES_1,
			render_size,
			view_count,
			1,
			true,
			false,
		);
	}
}
