shader_type spatial;
render_mode unshaded, world_vertex_coords, cull_disabled, depth_draw_always;

uniform sampler2D depth_texture : hint_depth_texture;
uniform sampler2D screen_texture : hint_screen_texture, repeat_disable;

uniform vec3 shallow_color : source_color;
uniform vec3 deep_color : source_color;

uniform float water_intensity = 1.0;
uniform float water_scale = 1.0;
uniform float water_fog_start;
uniform float water_fog_end;
uniform float water_transparency : hint_range(0.0, 20.0, 0.01) = 0.2;

global uniform sampler2D water_displacement;


void vertex() {
	vec3 displacement = (textureLod(water_displacement, VERTEX.xz / water_scale, 0).xyz * 2.0 - 1.0) * water_intensity;
	VERTEX.xyz += displacement;
}

void fragment() {
	vec2 screen_uv = SCREEN_UV;
	vec2 ndc = screen_uv * 2.0 - 1.0;

	float water_depth = VERTEX.z;

	float world_depth = textureLod(depth_texture, screen_uv, 0.0).r;
	vec4 world = INV_PROJECTION_MATRIX * vec4(ndc, world_depth, 1.0);
	world.xyz /= world.w;
	world_depth = world.z;

	if (FRONT_FACING) {
		// float water_thickness = water_depth - world_depth;

		// float time_scale = 1.0;
		// float refract_strength = 25.0;
		// vec2 refract_uv = screen_uv;
		// refract_uv.x += sin(TIME * time_scale + (refract_uv.x + refract_uv.y) ) * 0.01 * water_thickness;
		// refract_uv.y += cos(TIME * time_scale + (refract_uv.x - refract_uv.y) ) * 0.01 * water_thickness;

		// vec4 new_world = textureLod(depth_texture, refract_uv, 0.0);
		// new_world = INV_PROJECTION_MATRIX * vec4(ndc, new_world.r, 1.0);
		// new_world.xyz /= new_world.w;
		// float new_world_depth = new_world.z;

		// if (new_world_depth <= water_depth) {
		// 	screen_uv = refract_uv;
		// 	world = new_world;
		// 	world_depth = new_world_depth;
		// 	water_thickness = water_depth - world_depth;
		// }

		float fog_blend = smoothstep(world_depth, world_depth + water_fog_end, water_depth);
		fog_blend = clamp(exp(fog_blend * -water_transparency), 0.0, 1.0);

		float alpha_blend = smoothstep(world_depth, world_depth + water_fog_start, water_depth);
		alpha_blend = clamp(exp(alpha_blend * -water_transparency), 0.0, 1.0);

		vec3 world_color = textureLod(screen_texture, screen_uv, 0.0).rgb;
		world_color = mix(shallow_color, world_color, alpha_blend);
		world_color = mix(deep_color, world_color, fog_blend);


		ALBEDO = world_color;
	}
	else {
		// TODO
		ALBEDO = shallow_color;
		ALPHA = 1.0;
	}

	ROUGHNESS = 1.0;
}