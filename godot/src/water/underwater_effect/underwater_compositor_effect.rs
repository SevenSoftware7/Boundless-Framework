

use godot::builtin::{Array, Projection, Rid, StringName};
use godot::classes::compositor_effect::EffectCallbackType;
use godot::classes::notify::ObjectNotification;
use godot::classes::rendering_device::{
	CompareOperator, DataFormat, RenderPrimitive, SamplerFilter, SamplerRepeatMode, ShaderLanguage, ShaderStage, TextureSamples, TextureUsageBits
};
use godot::classes::{
	CompositorEffect, ICompositorEffect, RdAttachmentFormat, RdPipelineColorBlendState, RdPipelineColorBlendStateAttachment, RdPipelineDepthStencilState, RdPipelineMultisampleState, RdPipelineRasterizationState, RdSamplerState, RdShaderFile, RdShaderSource, RdVertexAttribute, RenderData, RenderSceneBuffersRd, RenderingDevice, RenderingServer, Texture2Drd
};
use godot::global::godot_error;
use godot::obj::{Base, EngineBitfield, Gd, NewGd, Singleton};
use godot::prelude::{godot_api, GodotClass};

use crate::water::{apply_effect_pass, mesh_data, render_mask_pass, textures};
use crate::rendering::groups_for_size;


pub(crate) const BUILTIN_WATER_MASK_VERTEX_GLSL: &str =
	include_str!("../../assets/shaders/RenderWaterMaskVertex.glsl");

pub(crate) const BUILTIN_WATER_MASK_FRAGMENT_GLSL: &str =
	include_str!("../../assets/shaders/RenderWaterMaskFragment.glsl");

pub(crate) const UNDERWATER_EFFECT_GLSL: &str =
	include_str!("../../assets/shaders/RenderUnderwaterEffect.glsl");


#[derive(GodotClass)]
#[class(base = CompositorEffect, tool)]
pub struct UnderwaterEffect {
	base: Base<CompositorEffect>,

	#[var(set = set_render_shader_file)]
	#[export]
	render_shader_file: Option<Gd<RdShaderFile>>,

	#[export]
	water_displacement_texture: Option<Gd<Texture2Drd>>,

	rendering_device: Option<Gd<RenderingDevice>>,
	render_shader: Rid,
	compute_shader: Rid,
	render_pipeline: Rid,
	compute_pipeline: Rid,
	displacement_sampler: Rid,
	depth_sampler: Rid,
	framebuffer_format: i64,
	vertex_format: i64,
}

#[godot_api]
impl ICompositorEffect for UnderwaterEffect {
	fn init(base: Base<CompositorEffect>) -> Self {
		let rendering_device = RenderingServer::singleton().get_rendering_device();
		if rendering_device.is_none() {
			godot_error!("UnderwaterEffect disabled: RenderingDevice unavailable");
		}

		let mut init_base = base.to_init_gd();
		init_base.set_effect_callback_type(EffectCallbackType::POST_TRANSPARENT);

		let mut effect = Self {
			base,
			render_shader_file: None,
			water_displacement_texture: None,
			rendering_device,
			render_shader: Rid::Invalid,
			compute_shader: Rid::Invalid,
			render_pipeline: Rid::Invalid,
			compute_pipeline: Rid::Invalid,
			displacement_sampler: Rid::Invalid,
			depth_sampler: Rid::Invalid,
			framebuffer_format: -1,
			vertex_format: -1,
		};

		if effect.rendering_device.is_none() {
			return effect;
		}

		effect.construct_samplers();
		effect.construct_render_pipeline();
		effect.construct_compute_pipeline();

		effect
	}

	fn on_notification(&mut self, what: ObjectNotification) {
		if what == ObjectNotification::PREDELETE {
			self.destruct();
		}
	}

	#[allow(clippy::too_many_lines)]
	fn render_callback(&mut self, _effect_callback_type: i32, render_data: Option<Gd<RenderData>>) {
		// if effect_callback_type != EffectCallbackType::POST_TRANSPARENT.ord() {
		// 	return;
		// }

		let Some(mesh_data) = mesh_data::collect_water_mesh_data() else {
			return;
		};
		if mesh_data.vertex_floats.is_empty() || mesh_data.indices.is_empty() {
			return;
		}

		let Some(rd) = self.rendering_device.as_mut() else {
			godot_error!("UnderwaterEffect render callback called but rendering_device is not available");
			return;
		};

		if !self.render_pipeline.is_valid() ||
		   !self.compute_pipeline.is_valid() ||
		   !self.render_shader.is_valid() ||
		   !self.compute_shader.is_valid()
		{
			godot_error!("UnderwaterEffect render callback called but effect is not properly initialized");
			return;
		}

		let Some(displacement_texture) = self
			.water_displacement_texture
			.as_ref()
			.map(|texture| texture.get_texture_rd_rid())
		else {
			godot_error!("UnderwaterEffect render callback called but water_displacement_texture is not set");
			return;
		};

		if !displacement_texture.is_valid() {
			godot_error!("UnderwaterEffect render callback called but water_displacement_texture is not valid");
			return;
		}

		let Some(render_data) = render_data else {
			godot_error!("UnderwaterEffect render callback called but render_data is not set");
			return;
		};
		let Some(scene_buffers_base) = render_data.get_render_scene_buffers() else {
			godot_error!("UnderwaterEffect render callback called but render_scene_buffers is not valid");
			return;
		};
		let Ok(mut scene_buffers) = scene_buffers_base.try_cast::<RenderSceneBuffersRd>() else {
			godot_error!("UnderwaterEffect render callback called but render_scene_buffers is not valid");
			return;
		};

		let Some(scene_data) = render_data.get_render_scene_data() else {
			godot_error!("UnderwaterEffect render callback called but render_scene_data is not valid");
			return;
		};

		let render_size = scene_buffers.get_internal_size();
		if render_size.x <= 0 || render_size.y <= 0 {
			godot_error!("UnderwaterEffect render callback called but render_size is invalid");
			return;
		}

		let view_count = scene_buffers.get_view_count();
		textures::ensure_water_map_textures(&mut scene_buffers, view_count, render_size);

		#[allow(clippy::cast_possible_truncation)]
		let vertex_count = (mesh_data.vertex_floats.len() / 4) as u32;
		let (vertex_buffer, vertex_array) = mesh_data::create_vertex_buffers(
			rd,
			&mesh_data.vertex_floats,
			self.vertex_format,
			vertex_count,
		);
		if !vertex_buffer.is_valid() || !vertex_array.is_valid() {
			if vertex_array.is_valid() {
				rd.free_rid(vertex_array);
			}
			if vertex_buffer.is_valid() {
				rd.free_rid(vertex_buffer);
			}
			return;
		}

		let (index_buffer, index_array) = mesh_data::create_index_buffers(rd, &mesh_data.indices);
		if !index_buffer.is_valid() || !index_array.is_valid() {
			rd.free_rid(vertex_array);
			rd.free_rid(vertex_buffer);
			if index_array.is_valid() {
				rd.free_rid(index_array);
			}
			if index_buffer.is_valid() {
				rd.free_rid(index_buffer);
			}
			return;
		}

		let info_buffer = mesh_data::create_storage_buffer(rd, &mesh_data.water_info_floats);
		let parameters_buffer = mesh_data::create_storage_buffer(rd, &mesh_data.water_params_floats);
		if !info_buffer.is_valid() || !parameters_buffer.is_valid() {
			rd.free_rid(vertex_array);
			rd.free_rid(vertex_buffer);
			rd.free_rid(index_array);
			rd.free_rid(index_buffer);
			if info_buffer.is_valid() {
				rd.free_rid(info_buffer);
			}
			if parameters_buffer.is_valid() {
				rd.free_rid(parameters_buffer);
			}
			return;
		}

		let (x_groups, y_groups) = groups_for_size(render_size, 8);
		let screen_w = (render_size.x - 1).max(1);
		let screen_h = (render_size.y - 1).max(1);

		for view in 0..view_count {
			let context = StringName::from(textures::UNDERWATER_CONTEXT);
			let map_name = StringName::from(textures::WATER_MAP_NAME);
			let depth_name = StringName::from(textures::WATER_DEPTH_NAME);

			let water_map = scene_buffers.get_texture_slice(&context, &map_name, view, 0, 1, 1);
			let water_depth = scene_buffers.get_texture_slice(&context, &depth_name, view, 0, 1, 1);
			if !water_map.is_valid() || !water_depth.is_valid() {
				continue;
			}

			let mut attachments = Array::<Rid>::new();
			attachments.push(water_map);
			attachments.push(water_depth);
			let water_framebuffer = rd
				.framebuffer_create_ex(&attachments)
				.validate_with_format(self.framebuffer_format)
				.done();
			if !water_framebuffer.is_valid() {
				continue;
			}

			let projection = scene_data.get_view_projection(view);
			let world_to_view = Projection::from(scene_data.get_cam_transform().affine_inverse());
			let world_to_clip = projection * world_to_view;

			let rendered_mask = render_mask_pass::run_render_mask_pass(
				rd,
				water_framebuffer,
				self.render_pipeline,
				self.render_shader,
				self.displacement_sampler,
				displacement_texture,
				info_buffer,
				vertex_array,
				index_array,
				world_to_clip,
			);
			if !rendered_mask {
				rd.free_rid(water_framebuffer);
				continue;
			}

			let color = scene_buffers.get_color_layer(view);
			let depth = scene_buffers.get_depth_layer(view);
			if !color.is_valid() || !depth.is_valid() {
				rd.free_rid(water_framebuffer);
				continue;
			}

			apply_effect_pass::run_apply_underwater_pass(
				rd,
				self.compute_pipeline,
				self.compute_shader,
				self.depth_sampler,
				color,
				depth,
				water_map,
				parameters_buffer,
				projection,
				(screen_w, screen_h),
				(x_groups, y_groups),
			);

			rd.free_rid(water_framebuffer);
		}

		rd.free_rid(vertex_array);
		rd.free_rid(vertex_buffer);
		rd.free_rid(index_array);
		rd.free_rid(index_buffer);
		rd.free_rid(info_buffer);
		rd.free_rid(parameters_buffer);
	}
}

#[godot_api]
impl UnderwaterEffect {
	fn construct_samplers(&mut self) {
		let Some(rendering_device) = self.rendering_device.as_mut() else {
			return;
		};
		let rd = rendering_device;

		let mut displacement_state = RdSamplerState::new_gd();
		displacement_state.set_min_filter(SamplerFilter::LINEAR);
		displacement_state.set_mag_filter(SamplerFilter::LINEAR);
		displacement_state.set_repeat_u(SamplerRepeatMode::REPEAT);
		displacement_state.set_repeat_v(SamplerRepeatMode::REPEAT);
		displacement_state.set_repeat_w(SamplerRepeatMode::REPEAT);
		self.displacement_sampler = rd.sampler_create(&displacement_state);

		let mut depth_state = RdSamplerState::new_gd();
		depth_state.set_min_filter(SamplerFilter::NEAREST);
		depth_state.set_mag_filter(SamplerFilter::NEAREST);
		self.depth_sampler = rd.sampler_create(&depth_state);
	}

	fn construct_render_pipeline(&mut self) {
		let Some(rd) = self.rendering_device.as_mut() else {
			return;
		};

		self.render_shader = if let Some(shader_file) = self.render_shader_file.as_ref() {
			shader_file.get_spirv()
				.map_or(Rid::Invalid, |spirv| rd.shader_create_from_spirv(&spirv))
		} else {
			let mut shader_source = RdShaderSource::new_gd();
			shader_source.set_language(ShaderLanguage::GLSL);
			shader_source.set_stage_source(ShaderStage::VERTEX, BUILTIN_WATER_MASK_VERTEX_GLSL);
			shader_source.set_stage_source(ShaderStage::FRAGMENT, BUILTIN_WATER_MASK_FRAGMENT_GLSL);

			rd.shader_compile_spirv_from_source(&shader_source)
				.map_or(Rid::Invalid, |spirv| rd.shader_create_from_spirv(&spirv))
		};
		if !self.render_shader.is_valid() {
			return;
		}

		let mut water_map_attachment = RdAttachmentFormat::new_gd();
		water_map_attachment.set_format(DataFormat::R32G32B32A32_SFLOAT);
		water_map_attachment.set_samples(TextureSamples::SAMPLES_1);
		#[allow(clippy::cast_possible_truncation)]
		water_map_attachment.set_usage_flags(
			(TextureUsageBits::COLOR_ATTACHMENT_BIT | TextureUsageBits::STORAGE_BIT).ord() as u32,
		);

		let mut water_depth_attachment = RdAttachmentFormat::new_gd();
		water_depth_attachment.set_format(DataFormat::D32_SFLOAT);
		water_depth_attachment.set_samples(TextureSamples::SAMPLES_1);
		#[allow(clippy::cast_possible_truncation)]
		water_depth_attachment.set_usage_flags(
			TextureUsageBits::DEPTH_STENCIL_ATTACHMENT_BIT.ord() as u32
		);

		let mut attachments = Array::<Gd<RdAttachmentFormat>>::new();
		attachments.push(&water_map_attachment);
		attachments.push(&water_depth_attachment);
		self.framebuffer_format = rd.framebuffer_format_create(&attachments);
		if self.framebuffer_format < 0 {
			return;
		}

		let mut vertex_attribute = RdVertexAttribute::new_gd();
		vertex_attribute.set_format(DataFormat::R32G32B32A32_SFLOAT);
		vertex_attribute.set_location(0);
		#[allow(clippy::cast_possible_truncation)]
		vertex_attribute.set_stride(4 * size_of::<f32>() as u32);
		vertex_attribute.set_offset(0);

		let mut attributes = Array::<Gd<RdVertexAttribute>>::new();
		attributes.push(&vertex_attribute);
		self.vertex_format = rd.vertex_format_create(&attributes);
		if self.vertex_format < 0 {
			return;
		}

		let mut blend_attachment = RdPipelineColorBlendStateAttachment::new_gd();
		blend_attachment.set_write_r(true);
		blend_attachment.set_write_g(true);
		blend_attachment.set_write_b(true);
		blend_attachment.set_write_a(true);
		blend_attachment.set_enable_blend(false);

		let mut blend_attachments = Array::<Gd<RdPipelineColorBlendStateAttachment>>::new();
		blend_attachments.push(&blend_attachment);

		let mut blend_state = RdPipelineColorBlendState::new_gd();
		blend_state.set_attachments(&blend_attachments);

		let mut depth_state = RdPipelineDepthStencilState::new_gd();
		depth_state.set_enable_depth_test(true);
		depth_state.set_enable_depth_write(true);
		depth_state.set_depth_compare_operator(CompareOperator::GREATER_OR_EQUAL);

		let raster_state = RdPipelineRasterizationState::new_gd();
		let multisample_state = RdPipelineMultisampleState::new_gd();

		self.render_pipeline = rd.render_pipeline_create(
			self.render_shader,
			self.framebuffer_format,
			self.vertex_format,
			RenderPrimitive::TRIANGLES,
			&raster_state,
			&multisample_state,
			&depth_state,
			&blend_state,
		);
	}

	fn destruct_render_pipeline(&mut self) {
		let Some(rd) = self.rendering_device.as_mut() else {
			return;
		};

		if self.render_pipeline.is_valid() {
			rd.free_rid(self.render_pipeline);
			self.render_pipeline = Rid::Invalid;
		}
		if self.render_shader.is_valid() {
			rd.free_rid(self.render_shader);
			self.render_shader = Rid::Invalid;
		}
	}

	fn construct_compute_pipeline(&mut self) {
		let Some(rd) = self.rendering_device.as_mut() else {
			return;
		};

		let mut shader_source = RdShaderSource::new_gd();
		shader_source.set_language(ShaderLanguage::GLSL);
		shader_source.set_stage_source(ShaderStage::COMPUTE, UNDERWATER_EFFECT_GLSL);

		self.compute_shader = rd.shader_compile_spirv_from_source(&shader_source)
			.map_or(Rid::Invalid, |spirv| rd.shader_create_from_spirv(&spirv));

		if self.compute_shader.is_valid() {
			self.compute_pipeline = rd.compute_pipeline_create(self.compute_shader);
		}
	}

	fn destruct(&mut self) {
		let Some(rd) = self.rendering_device.as_mut() else {
			return;
		};

		if self.displacement_sampler.is_valid() {
			rd.free_rid(self.displacement_sampler);
			self.displacement_sampler = Rid::Invalid;
		}
		if self.depth_sampler.is_valid() {
			rd.free_rid(self.depth_sampler);
			self.depth_sampler = Rid::Invalid;
		}
		if self.render_pipeline.is_valid() {
			rd.free_rid(self.render_pipeline);
			self.render_pipeline = Rid::Invalid;
		}
		if self.render_shader.is_valid() {
			rd.free_rid(self.render_shader);
			self.render_shader = Rid::Invalid;
		}
		if self.compute_pipeline.is_valid() {
			rd.free_rid(self.compute_pipeline);
			self.compute_pipeline = Rid::Invalid;
		}
		if self.compute_shader.is_valid() {
			rd.free_rid(self.compute_shader);
			self.compute_shader = Rid::Invalid;
		}
		self.framebuffer_format = -1;
		self.vertex_format = -1;
	}

	#[func]
	fn set_render_shader_file(&mut self, shader_file: Option<Gd<RdShaderFile>>) {
		self.destruct_render_pipeline();
		self.render_shader_file = shader_file;
		self.construct_render_pipeline();
	}
}
