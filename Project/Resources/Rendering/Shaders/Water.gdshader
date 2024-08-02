shader_type spatial;
render_mode world_vertex_coords, depth_draw_always;

uniform vec3 shallow_color : source_color;
uniform vec3 deep_color : source_color;

uniform float water_intensity;
uniform float water_scale;

global uniform sampler2D water_displacement;

void vertex() {
	vec3 water_data = (textureLod(water_displacement, VERTEX.xz / water_scale, 0).xyz * 2.0 - 1.0) * water_intensity;
	VERTEX.xyz += water_data;
}

void fragment() {
	ALBEDO = shallow_color;
	ALPHA = 0.95;
}