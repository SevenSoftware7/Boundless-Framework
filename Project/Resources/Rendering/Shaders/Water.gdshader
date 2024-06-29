shader_type spatial;
render_mode world_vertex_coords;

global uniform vec4 water_color;
global uniform float water_scale;
global uniform float water_intensity;
global uniform sampler2D water_displacement;

void vertex() {
	VERTEX.xyz += textureLod(water_displacement, VERTEX.xz / water_scale, 0).rgb * water_intensity;
}

void fragment() {
	ALBEDO = water_color.rgb;
	ALPHA = 0.6;
}