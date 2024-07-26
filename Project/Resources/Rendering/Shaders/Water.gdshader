shader_type spatial;
render_mode world_vertex_coords, depth_draw_always;

global uniform vec4 water_color;
global uniform float water_scale;
global uniform float water_intensity;
global uniform sampler2D water_displacement;

void vertex() {
	vec3 water_data = (textureLod(water_displacement, VERTEX.xz / water_scale, 0).xyz * 2.0 - 1.0) * water_intensity;
	VERTEX.xyz += water_data;
}

void fragment() {
	ALBEDO = water_color.rgb;
	ALPHA = 0.95;
}